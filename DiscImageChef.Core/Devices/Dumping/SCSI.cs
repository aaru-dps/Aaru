// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SCSI.cs
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
    public class SCSI
    {
        // TODO: Get cartridge serial number from Certance vendor EVPD
        public static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force, bool dumpRaw, bool persistent, bool stopOnError)
        {
            bool aborted;
            MHDDLog mhddLog;
            IBGLog ibgLog;
            byte[] cmdBuf = null;
            byte[] senseBuf = null;
            bool sense = false;
            double duration;
            ulong blocks = 0;
            uint blockSize = 0;
            byte[] tmpBuf;
            MediaType dskType = MediaType.Unknown;
            bool opticalDisc = false;
            uint logicalBlockSize = 0;
            uint physicalBlockSize = 0;

            if(dev.IsRemovable)
            {
                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                if(sense)
                {
                    Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                    if(decSense.HasValue)
                    {
                        if(decSense.Value.ASC == 0x3A)
                        {
                            int leftRetries = 5;
                            while(leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                System.Threading.Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                                if(!sense)
                                    break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Please insert media in drive");
                                return;
                            }
                        }
                        else if(decSense.Value.ASC == 0x04 && decSense.Value.ASCQ == 0x01)
                        {
                            int leftRetries = 10;
                            while(leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                System.Threading.Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                                if(!sense)
                                    break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                                return;
                            }
                        }
                        /*else if (decSense.Value.ASC == 0x29 && decSense.Value.ASCQ == 0x00)
                        {
                            if (!deviceReset)
                            {
                                deviceReset = true;
                                DicConsole.ErrorWriteLine("Device did reset, retrying...");
                                goto retryTestReady;
                            }

                            DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                            return;
                        }*/
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

            Reader scsiReader = null;
            if(dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.DirectAccess ||
                dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice ||
                dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.OCRWDevice ||
                dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.OpticalDevice ||
                dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.SimplifiedDevice ||
                dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.WriteOnceDevice)
            {
                scsiReader = new Reader(dev, dev.Timeout, null, dumpRaw);
                blocks = scsiReader.GetDeviceBlocks();
                blockSize = scsiReader.LogicalBlockSize;
                if(scsiReader.FindReadCommand())
                {
                    DicConsole.ErrorWriteLine("Unable to read medium.");
                    return;
                }

                if(blocks != 0 && blockSize != 0)
                {
                    blocks++;
                    DicConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2} bytes)",
                        blocks, blockSize, blocks * (ulong)blockSize);
                }

                logicalBlockSize = blockSize;
                physicalBlockSize = scsiReader.PhysicalBlockSize;
            }

            DateTime start;
            DateTime end;
            double totalDuration = 0;
            double totalChkDuration = 0;
            double currentSpeed = 0;
            double maxSpeed = double.MinValue;
            double minSpeed = double.MaxValue;
            List<ulong> unreadableSectors = new List<ulong>();
            Checksum dataChk;
            CICMMetadataType sidecar = new CICMMetadataType();

            DataFile dumpFile = null;

            if(dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
            {
                if(dumpRaw)
                    throw new ArgumentException("Tapes cannot be dumped raw.");

                Decoders.SCSI.FixedSense? fxSense;
                string strSense;
                byte[] tapeBuf;

                dev.RequestSense(out senseBuf, dev.Timeout, out duration);
                fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);

                if(fxSense.HasValue && fxSense.Value.SenseKey != Decoders.SCSI.SenseKeys.NoSense)
                {
                    DicConsole.ErrorWriteLine("Drive has status error, please correct. Sense follows...");
                    DicConsole.ErrorWriteLine("{0}", strSense);
                    return;
                }

                // Not in BOM/P
                if(fxSense.HasValue && fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ != 0x04)
                {
                    DicConsole.Write("Rewinding, please wait...");
                    // Rewind, let timeout apply
                    sense = dev.Rewind(out senseBuf, dev.Timeout, out duration);

                    // Still rewinding?
                    // TODO: Pause?
                    do
                    {
                        DicConsole.Write("\rRewinding, please wait...");
                        dev.RequestSense(out senseBuf, dev.Timeout, out duration);
                        fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                    }
                    while(fxSense.HasValue && fxSense.Value.ASC == 0x00 && (fxSense.Value.ASCQ == 0x1A || fxSense.Value.ASCQ != 0x04));

                    dev.RequestSense(out senseBuf, dev.Timeout, out duration);
                    fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);

                    // And yet, did not rewind!
                    if(fxSense.HasValue && ((fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ != 0x04) || fxSense.Value.ASC != 0x00))
                    {
                        DicConsole.WriteLine();
                        DicConsole.ErrorWriteLine("Drive could not rewind, please correct. Sense follows...");
                        DicConsole.ErrorWriteLine("{0}", strSense);
                        return;
                    }

                    DicConsole.WriteLine();
                }

                // Check position
                sense = dev.ReadPosition(out tapeBuf, out senseBuf, SscPositionForms.Short, dev.Timeout, out duration);

                if(sense)
                {
                    // READ POSITION is mandatory starting SCSI-2, so do not cry if the drive does not recognize the command (SCSI-1 or earlier)
                    // Anyway, <=SCSI-1 tapes do not support partitions
                    fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                    if(fxSense.HasValue && ((fxSense.Value.ASC == 0x20 && fxSense.Value.ASCQ != 0x00) || fxSense.Value.ASC != 0x20))
                    {
                        DicConsole.ErrorWriteLine("Could not get position. Sense follows...");
                        DicConsole.ErrorWriteLine("{0}", strSense);
                        return;
                    }
                }
                else
                {
                    // Not in partition 0
                    if(tapeBuf[1] != 0)
                    {
                        DicConsole.Write("Drive not in partition 0. Rewinding, please wait...");
                        // Rewind, let timeout apply
                        sense = dev.Locate(out senseBuf, false, 0, 0, dev.Timeout, out duration);
                        if(sense)
                        {
                            DicConsole.WriteLine();
                            DicConsole.ErrorWriteLine("Drive could not rewind, please correct. Sense follows...");
                            DicConsole.ErrorWriteLine("{0}", strSense);
                            return;
                        }

                        // Still rewinding?
                        // TODO: Pause?
                        do
                        {
                            System.Threading.Thread.Sleep(1000);
                            DicConsole.Write("\rRewinding, please wait...");
                            dev.RequestSense(out senseBuf, dev.Timeout, out duration);
                            fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                        }
                        while(fxSense.HasValue && fxSense.Value.ASC == 0x00 && (fxSense.Value.ASCQ == 0x1A || fxSense.Value.ASCQ == 0x19));

                        // And yet, did not rewind!
                        if(fxSense.HasValue && ((fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ != 0x04) || fxSense.Value.ASC != 0x00))
                        {
                            DicConsole.WriteLine();
                            DicConsole.ErrorWriteLine("Drive could not rewind, please correct. Sense follows...");
                            DicConsole.ErrorWriteLine("{0}", strSense);
                            return;
                        }

                        sense = dev.ReadPosition(out tapeBuf, out senseBuf, SscPositionForms.Short, dev.Timeout, out duration);
                        if(sense)
                        {
                            fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                            DicConsole.ErrorWriteLine("Drive could not rewind, please correct. Sense follows...");
                            DicConsole.ErrorWriteLine("{0}", strSense);
                            return;
                        }

                        // Still not in partition 0!!!?
                        if(tapeBuf[1] != 0)
                        {
                            DicConsole.ErrorWriteLine("Drive could not rewind to partition 0 but no error occurred...");
                            return;
                        }

                        DicConsole.WriteLine();
                    }
                }

                sidecar.BlockMedia = new BlockMediaType[1];
                sidecar.BlockMedia[0] = new BlockMediaType();
                sidecar.BlockMedia[0].SCSI = new SCSIType();
                byte scsiMediumTypeTape = 0;
                byte scsiDensityCodeTape = 0;

                sense = dev.ModeSense10(out cmdBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F, 0xFF, 5, out duration);
                if(!sense || dev.Error)
                {
                    sense = dev.ModeSense10(out cmdBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out duration);
                }

                Decoders.SCSI.Modes.DecodedMode? decMode = null;

                if(!sense && !dev.Error)
                {
                    if(Decoders.SCSI.Modes.DecodeMode10(cmdBuf, dev.SCSIType).HasValue)
                    {
                        decMode = Decoders.SCSI.Modes.DecodeMode10(cmdBuf, dev.SCSIType);
                        sidecar.BlockMedia[0].SCSI.ModeSense10 = new DumpType();
                        sidecar.BlockMedia[0].SCSI.ModeSense10.Image = outputPrefix + ".modesense10.bin";
                        sidecar.BlockMedia[0].SCSI.ModeSense10.Size = cmdBuf.Length;
                        sidecar.BlockMedia[0].SCSI.ModeSense10.Checksums = Checksum.GetChecksums(cmdBuf).ToArray();
                        DataFile.WriteTo("SCSI Dump", sidecar.BlockMedia[0].SCSI.ModeSense10.Image, cmdBuf);
                    }
                }

                sense = dev.ModeSense6(out cmdBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out duration);
                if(sense || dev.Error)
                    sense = dev.ModeSense6(out cmdBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out duration);
                if(sense || dev.Error)
                    sense = dev.ModeSense(out cmdBuf, out senseBuf, 5, out duration);

                if(!sense && !dev.Error)
                {
                    if(Decoders.SCSI.Modes.DecodeMode6(cmdBuf, dev.SCSIType).HasValue)
                    {
                        decMode = Decoders.SCSI.Modes.DecodeMode6(cmdBuf, dev.SCSIType);
                        sidecar.BlockMedia[0].SCSI.ModeSense = new DumpType();
                        sidecar.BlockMedia[0].SCSI.ModeSense.Image = outputPrefix + ".modesense.bin";
                        sidecar.BlockMedia[0].SCSI.ModeSense.Size = cmdBuf.Length;
                        sidecar.BlockMedia[0].SCSI.ModeSense.Checksums = Checksum.GetChecksums(cmdBuf).ToArray();
                        DataFile.WriteTo("SCSI Dump", sidecar.BlockMedia[0].SCSI.ModeSense.Image, cmdBuf);
                    }
                }

                // TODO: Check partitions page
                if(decMode.HasValue)
                {
                    scsiMediumTypeTape = (byte)decMode.Value.Header.MediumType;
                    if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length >= 1)
                        scsiDensityCodeTape = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                }

                if(dskType == MediaType.Unknown)
                    dskType = MediaTypeFromSCSI.Get((byte)dev.SCSIType, dev.Manufacturer, dev.Model, scsiMediumTypeTape, scsiDensityCodeTape, blocks, blockSize);

                DicConsole.WriteLine("Media identified as {0}", dskType);

                bool endOfMedia = false;
                ulong currentBlock = 0;
                ulong currentFile = 0;
                byte currentPartition = 0;
                byte totalPartitions = 1; // TODO: Handle partitions.
                blockSize = 1;
                ulong currentSize = 0;
                ulong currentPartitionSize = 0;
                ulong currentFileSize = 0;

                Checksum partitionChk;
                Checksum fileChk;
                List<TapePartitionType> partitions = new List<TapePartitionType>();
                List<TapeFileType> files = new List<TapeFileType>();
                TapeFileType currentTapeFile = new TapeFileType();
                TapePartitionType currentTapePartition = new TapePartitionType();

                DicConsole.WriteLine();
                dumpFile = new DataFile(outputPrefix + ".bin");
                dataChk = new Checksum();
                start = DateTime.UtcNow;
                mhddLog = new MHDDLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, 1);
                ibgLog = new IBGLog(outputPrefix + ".ibg", 0x0008);

                currentTapeFile = new TapeFileType();
                currentTapeFile.Image = new ImageType();
                currentTapeFile.Image.format = "BINARY";
                currentTapeFile.Image.offset = (long)currentSize;
                currentTapeFile.Image.offsetSpecified = true;
                currentTapeFile.Image.Value = outputPrefix + ".bin";
                currentTapeFile.Sequence = (long)currentFile;
                currentTapeFile.StartBlock = (long)currentBlock;
                fileChk = new Checksum();
                currentTapePartition = new TapePartitionType();
                currentTapePartition.Image = new ImageType();
                currentTapePartition.Image.format = "BINARY";
                currentTapePartition.Image.offset = (long)currentSize;
                currentTapePartition.Image.offsetSpecified = true;
                currentTapePartition.Image.Value = outputPrefix + ".bin";
                currentTapePartition.Sequence = (long)currentPartition;
                currentTapePartition.StartBlock = (long)currentBlock;
                partitionChk = new Checksum();

                aborted = false;
                System.Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = aborted = true;
                };

                while(currentPartition < totalPartitions)
                {
                    if(aborted)
                        break;

                    if(endOfMedia)
                    {
                        DicConsole.WriteLine();
                        DicConsole.WriteLine("Finished partition {0}", currentPartition);
                        currentTapePartition.File = files.ToArray();
                        currentTapePartition.Checksums = partitionChk.End().ToArray();
                        currentTapePartition.EndBlock = (long)(currentBlock - 1);
                        currentTapePartition.Size = (long)currentPartitionSize;
                        partitions.Add(currentTapePartition);

                        currentPartition++;

                        if(currentPartition < totalPartitions)
                        {
                            currentFile++;
                            currentTapeFile = new TapeFileType();
                            currentTapeFile.Image = new ImageType();
                            currentTapeFile.Image.format = "BINARY";
                            currentTapeFile.Image.offset = (long)currentSize;
                            currentTapeFile.Image.offsetSpecified = true;
                            currentTapeFile.Image.Value = outputPrefix + ".bin";
                            currentTapeFile.Sequence = (long)currentFile;
                            currentTapeFile.StartBlock = (long)currentBlock;
                            currentFileSize = 0;
                            fileChk = new Checksum();
                            files = new List<TapeFileType>();
                            currentTapePartition = new TapePartitionType();
                            currentTapePartition.Image = new ImageType();
                            currentTapePartition.Image.format = "BINARY";
                            currentTapePartition.Image.offset = (long)currentSize;
                            currentTapePartition.Image.offsetSpecified = true;
                            currentTapePartition.Image.Value = outputPrefix + ".bin";
                            currentTapePartition.Sequence = currentPartition;
                            currentTapePartition.StartBlock = (long)currentBlock;
                            currentPartitionSize = 0;
                            partitionChk = new Checksum();
                            DicConsole.WriteLine("Seeking to partition {0}", currentPartition);
                            sense = dev.Locate(out senseBuf, false, currentPartition, 0, dev.Timeout, out duration);
                            totalDuration += duration;
                        }

                        continue;
                    }

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0)
                        maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0)
                        minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    DicConsole.Write("\rReading block {0} ({1:F3} MiB/sec.)", currentBlock, currentSpeed);

                    sense = dev.Read6(out tapeBuf, out senseBuf, false, blockSize, blockSize, dev.Timeout, out duration);
                    totalDuration += duration;

                    if(sense)
                    {
                        fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                        if(fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ == 0x00 && fxSense.Value.ILI && fxSense.Value.InformationValid)
                        {
                            blockSize = (uint)((int)blockSize - BitConverter.ToInt32(BitConverter.GetBytes(fxSense.Value.Information), 0));

                            DicConsole.WriteLine();
                            DicConsole.WriteLine("Blocksize changed to {0} bytes at block {1}", blockSize, currentBlock);

                            sense = dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, dev.Timeout, out duration);
                            totalDuration += duration;

                            if(sense)
                            {
                                fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                                DicConsole.WriteLine();
                                DicConsole.ErrorWriteLine("Drive could not go back one block. Sense follows...");
                                DicConsole.ErrorWriteLine("{0}", strSense);
                                dumpFile.Close();
                                return;
                            }

                            continue;
                        }

                        if(fxSense.Value.SenseKey == Decoders.SCSI.SenseKeys.BlankCheck)
                        {
                            if(currentBlock == 0)
                            {
                                DicConsole.WriteLine();
                                DicConsole.ErrorWriteLine("Cannot dump a blank tape...");
                                dumpFile.Close();
                                return;
                            }

                            // For sure this is an end-of-tape/partition
                            if(fxSense.Value.ASC == 0x00 && (fxSense.Value.ASCQ == 0x02 || fxSense.Value.ASCQ == 0x05))
                            {
                                // TODO: Detect end of partition
                                endOfMedia = true;
                                continue;
                            }

                            DicConsole.WriteLine();
                            DicConsole.WriteLine("Blank block found, end of tape?");
                            endOfMedia = true;
                            continue;
                        }

                        if((fxSense.Value.SenseKey == Decoders.SCSI.SenseKeys.NoSense || fxSense.Value.SenseKey == Decoders.SCSI.SenseKeys.RecoveredError) &&
                           (fxSense.Value.ASCQ == 0x02 || fxSense.Value.ASCQ == 0x05))
                        {
                            // TODO: Detect end of partition
                            endOfMedia = true;
                            continue;
                        }

                        if((fxSense.Value.SenseKey == Decoders.SCSI.SenseKeys.NoSense || fxSense.Value.SenseKey == Decoders.SCSI.SenseKeys.RecoveredError) &&
                           fxSense.Value.ASCQ == 0x01)
                        {
                            currentTapeFile.Checksums = fileChk.End().ToArray();
                            currentTapeFile.EndBlock = (long)(currentBlock - 1);
                            currentTapeFile.Size = (long)currentFileSize;
                            files.Add(currentTapeFile);

                            currentFile++;
                            currentTapeFile = new TapeFileType();
                            currentTapeFile.Image = new ImageType();
                            currentTapeFile.Image.format = "BINARY";
                            currentTapeFile.Image.offset = (long)currentSize;
                            currentTapeFile.Image.offsetSpecified = true;
                            currentTapeFile.Image.Value = outputPrefix + ".bin";
                            currentTapeFile.Sequence = (long)currentFile;
                            currentTapeFile.StartBlock = (long)currentBlock;
                            currentFileSize = 0;
                            fileChk = new Checksum();

                            DicConsole.WriteLine();
                            DicConsole.WriteLine("Changed to file {0} at block {1}", currentFile, currentBlock);
                            continue;
                        }

                        // TODO: Add error recovering for tapes
                        fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                        DicConsole.ErrorWriteLine("Drive could not read block. Sense follows...");
                        DicConsole.ErrorWriteLine("{0}", strSense);
                        return;
                    }

                    mhddLog.Write(currentBlock, duration);
                    ibgLog.Write(currentBlock, currentSpeed * 1024);
                    dumpFile.Write(tapeBuf);

                    DateTime chkStart = DateTime.UtcNow;
                    dataChk.Update(tapeBuf);
                    fileChk.Update(tapeBuf);
                    partitionChk.Update(tapeBuf);
                    DateTime chkEnd = DateTime.UtcNow;
                    double chkDuration = (chkEnd - chkStart).TotalMilliseconds;
                    totalChkDuration += chkDuration;

                    if(currentBlock % 10 == 0)
                        currentSpeed = ((double)2448 / (double)1048576) / (duration / (double)1000);
                    currentBlock++;
                    currentSize += blockSize;
                    currentFileSize += blockSize;
                    currentPartitionSize += blockSize;
                }

                blocks = currentBlock + 1;
                DicConsole.WriteLine();
                end = DateTime.UtcNow;
                mhddLog.Close();
                ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);

                DicConsole.WriteLine("Took a total of {0:F3} seconds ({1:F3} processing commands, {2:F3} checksumming).", (end - start).TotalSeconds, totalDuration / 1000, totalChkDuration / 1000);
#pragma warning disable IDE0004 // Cast is necessary, otherwise incorrect value is created
                DicConsole.WriteLine("Avegare speed: {0:F3} MiB/sec.", (((double)blockSize * (double)(blocks + 1)) / 1048576) / (totalDuration / 1000));
#pragma warning restore IDE0004 // Cast is necessary, otherwise incorrect value is created
                DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", maxSpeed);
                DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", minSpeed);

                sidecar.BlockMedia[0].Checksums = dataChk.End().ToArray();
                sidecar.BlockMedia[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(dskType);
                string xmlDskTyp, xmlDskSubTyp;
                Metadata.MediaType.MediaTypeToString(dskType, out xmlDskTyp, out xmlDskSubTyp);
                sidecar.BlockMedia[0].DiskType = xmlDskTyp;
                sidecar.BlockMedia[0].DiskSubType = xmlDskSubTyp;
                // TODO: Implement device firmware revision
                sidecar.BlockMedia[0].Image = new ImageType();
                sidecar.BlockMedia[0].Image.format = "Raw disk image (sector by sector copy)";
                sidecar.BlockMedia[0].Image.Value = outputPrefix + ".bin";
                sidecar.BlockMedia[0].LogicalBlocks = (long)blocks;
                sidecar.BlockMedia[0].Size = (long)(currentSize);
                sidecar.BlockMedia[0].DumpHardwareArray = new DumpHardwareType[1];
                sidecar.BlockMedia[0].DumpHardwareArray[0] = new DumpHardwareType();
                sidecar.BlockMedia[0].DumpHardwareArray[0].Extents = new ExtentType[1];
                sidecar.BlockMedia[0].DumpHardwareArray[0].Extents[0] = new ExtentType();
                sidecar.BlockMedia[0].DumpHardwareArray[0].Extents[0].Start = 0;
                sidecar.BlockMedia[0].DumpHardwareArray[0].Extents[0].End = (int)(blocks - 1);
                sidecar.BlockMedia[0].DumpHardwareArray[0].Manufacturer = dev.Manufacturer;
                sidecar.BlockMedia[0].DumpHardwareArray[0].Model = dev.Model;
                sidecar.BlockMedia[0].DumpHardwareArray[0].Revision = dev.Revision;
                sidecar.BlockMedia[0].DumpHardwareArray[0].Serial = dev.Serial;
                sidecar.BlockMedia[0].DumpHardwareArray[0].Software = new SoftwareType();
                sidecar.BlockMedia[0].DumpHardwareArray[0].Software.Name = "DiscImageChef";
                sidecar.BlockMedia[0].DumpHardwareArray[0].Software.OperatingSystem = dev.PlatformID.ToString();
                sidecar.BlockMedia[0].DumpHardwareArray[0].Software.Version = typeof(SCSI).Assembly.GetName().Version.ToString();
                sidecar.BlockMedia[0].TapeInformation = partitions.ToArray();

                if(!aborted)
                {
                    DicConsole.WriteLine("Writing metadata sidecar");

                    FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml",
                                           FileMode.Create);

                    System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(CICMMetadataType));
                    xmlSer.Serialize(xmlFs, sidecar);
                    xmlFs.Close();
                }

                Statistics.AddMedia(dskType, true);

                return;
            }

            if(blocks == 0)
            {
                DicConsole.ErrorWriteLine("Unable to read medium or empty medium present...");
                return;
            }

            bool compactDisc = true;
            Decoders.CD.FullTOC.CDFullTOC? toc = null;
            byte scsiMediumType = 0;
            byte scsiDensityCode = 0;
            bool containsFloppyPage = false;
            ushort currentProfile = 0x0001;
            bool isXbox = false;

            #region MultiMediaDevice
            if(dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
            {
                sidecar.OpticalDisc = new OpticalDiscType[1];
                sidecar.OpticalDisc[0] = new OpticalDiscType();
                opticalDisc = true;

                sense = dev.GetConfiguration(out cmdBuf, out senseBuf, 0, MmcGetConfigurationRt.Current, dev.Timeout, out duration);
                if(!sense)
                {
                    Decoders.SCSI.MMC.Features.SeparatedFeatures ftr = Decoders.SCSI.MMC.Features.Separate(cmdBuf);
                    currentProfile = ftr.CurrentProfile;

                    switch(ftr.CurrentProfile)
                    {
                        case 0x0001:
                            dskType = MediaType.GENERIC_HDD;
                            goto default;
                        case 0x0005:
                            dskType = MediaType.CDMO;
                            break;
                        case 0x0008:
                            dskType = MediaType.CD;
                            break;
                        case 0x0009:
                            dskType = MediaType.CDR;
                            break;
                        case 0x000A:
                            dskType = MediaType.CDRW;
                            break;
                        case 0x0010:
                            dskType = MediaType.DVDROM;
                            goto default;
                        case 0x0011:
                            dskType = MediaType.DVDR;
                            goto default;
                        case 0x0012:
                            dskType = MediaType.DVDRAM;
                            goto default;
                        case 0x0013:
                        case 0x0014:
                            dskType = MediaType.DVDRW;
                            goto default;
                        case 0x0015:
                        case 0x0016:
                            dskType = MediaType.DVDRDL;
                            goto default;
                        case 0x0017:
                            dskType = MediaType.DVDRWDL;
                            goto default;
                        case 0x0018:
                            dskType = MediaType.DVDDownload;
                            goto default;
                        case 0x001A:
                            dskType = MediaType.DVDPRW;
                            goto default;
                        case 0x001B:
                            dskType = MediaType.DVDPR;
                            goto default;
                        case 0x0020:
                            dskType = MediaType.DDCD;
                            goto default;
                        case 0x0021:
                            dskType = MediaType.DDCDR;
                            goto default;
                        case 0x0022:
                            dskType = MediaType.DDCDRW;
                            goto default;
                        case 0x002A:
                            dskType = MediaType.DVDPRWDL;
                            goto default;
                        case 0x002B:
                            dskType = MediaType.DVDPRDL;
                            goto default;
                        case 0x0040:
                            dskType = MediaType.BDROM;
                            goto default;
                        case 0x0041:
                        case 0x0042:
                            dskType = MediaType.BDR;
                            goto default;
                        case 0x0043:
                            dskType = MediaType.BDRE;
                            goto default;
                        case 0x0050:
                            dskType = MediaType.HDDVDROM;
                            goto default;
                        case 0x0051:
                            dskType = MediaType.HDDVDR;
                            goto default;
                        case 0x0052:
                            dskType = MediaType.HDDVDRAM;
                            goto default;
                        case 0x0053:
                            dskType = MediaType.HDDVDRW;
                            goto default;
                        case 0x0058:
                            dskType = MediaType.HDDVDRDL;
                            goto default;
                        case 0x005A:
                            dskType = MediaType.HDDVDRWDL;
                            goto default;
                        default:
                            compactDisc = false;
                            break;
                    }
                }

                #region CompactDisc
                if(compactDisc)
                {
                    // We discarded all discs that falsify a TOC before requesting a real TOC
                    // No TOC, no CD (or an empty one)
                    bool tocSense = dev.ReadRawToc(out cmdBuf, out senseBuf, 1, dev.Timeout, out duration);
                    if(!tocSense)
                    {
                        toc = Decoders.CD.FullTOC.Decode(cmdBuf);
                        if(toc.HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 2];
                            Array.Copy(cmdBuf, 2, tmpBuf, 0, cmdBuf.Length - 2);
                            sidecar.OpticalDisc[0].TOC = new DumpType();
                            sidecar.OpticalDisc[0].TOC.Image = outputPrefix + ".toc.bin";
                            sidecar.OpticalDisc[0].TOC.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].TOC.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].TOC.Image, tmpBuf);

                            // ATIP exists on blank CDs
                            sense = dev.ReadAtip(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                            if(!sense)
                            {
                                Decoders.CD.ATIP.CDATIP? atip = Decoders.CD.ATIP.Decode(cmdBuf);
                                if(atip.HasValue)
                                {
                                    if(blocks == 0)
                                    {
                                        DicConsole.ErrorWriteLine("Cannot dump blank media.");
                                        return;
                                    }

                                    // Only CD-R and CD-RW have ATIP
                                    dskType = atip.Value.DiscType ? MediaType.CDRW : MediaType.CDR;

                                    tmpBuf = new byte[cmdBuf.Length - 4];
                                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                                    sidecar.OpticalDisc[0].ATIP = new DumpType();
                                    sidecar.OpticalDisc[0].ATIP.Image = outputPrefix + ".atip.bin";
                                    sidecar.OpticalDisc[0].ATIP.Size = tmpBuf.Length;
                                    sidecar.OpticalDisc[0].ATIP.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].TOC.Image, tmpBuf);
                                }
                            }

                            sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf, MmcDiscInformationDataTypes.DiscInformation, dev.Timeout, out duration);
                            if(!sense)
                            {
                                Decoders.SCSI.MMC.DiscInformation.StandardDiscInformation? discInfo = Decoders.SCSI.MMC.DiscInformation.Decode000b(cmdBuf);
                                if(discInfo.HasValue)
                                {
                                    // If it is a read-only CD, check CD type if available
                                    if(dskType == MediaType.CD)
                                    {
                                        switch(discInfo.Value.DiscType)
                                        {
                                            case 0x10:
                                                dskType = MediaType.CDI;
                                                break;
                                            case 0x20:
                                                dskType = MediaType.CDROMXA;
                                                break;
                                        }
                                    }
                                }
                            }

                            int sessions = 1;
                            int firstTrackLastSession = 0;

                            sense = dev.ReadSessionInfo(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                            if(!sense)
                            {
                                Decoders.CD.Session.CDSessionInfo? session = Decoders.CD.Session.Decode(cmdBuf);
                                if(session.HasValue)
                                {
                                    sessions = session.Value.LastCompleteSession;
                                    firstTrackLastSession = session.Value.TrackDescriptors[0].TrackNumber;
                                }
                            }

                            if(dskType == MediaType.CD)
                            {
                                bool hasDataTrack = false;
                                bool hasAudioTrack = false;
                                bool allFirstSessionTracksAreAudio = true;
                                bool hasVideoTrack = false;

                                if(toc.HasValue)
                                {
                                    foreach(Decoders.CD.FullTOC.TrackDataDescriptor track in toc.Value.TrackDescriptors)
                                    {
                                        if(track.TNO == 1 &&
                                            ((Decoders.CD.TOC_CONTROL)(track.CONTROL & 0x0D) == Decoders.CD.TOC_CONTROL.DataTrack ||
                                            (Decoders.CD.TOC_CONTROL)(track.CONTROL & 0x0D) == Decoders.CD.TOC_CONTROL.DataTrackIncremental))
                                        {
                                            allFirstSessionTracksAreAudio &= firstTrackLastSession != 1;
                                        }

                                        if((Decoders.CD.TOC_CONTROL)(track.CONTROL & 0x0D) == Decoders.CD.TOC_CONTROL.DataTrack ||
                                            (Decoders.CD.TOC_CONTROL)(track.CONTROL & 0x0D) == Decoders.CD.TOC_CONTROL.DataTrackIncremental)
                                        {
                                            hasDataTrack = true;
                                            allFirstSessionTracksAreAudio &= track.TNO >= firstTrackLastSession;
                                        }
                                        else
                                            hasAudioTrack = true;

                                        hasVideoTrack |= track.ADR == 4;
                                    }
                                }

                                if(hasDataTrack && hasAudioTrack && allFirstSessionTracksAreAudio && sessions == 2)
                                    dskType = MediaType.CDPLUS;
                                if(!hasDataTrack && hasAudioTrack && sessions == 1)
                                    dskType = MediaType.CDDA;
                                if(hasDataTrack && !hasAudioTrack && sessions == 1)
                                    dskType = MediaType.CDROM;
                                if(hasVideoTrack && !hasDataTrack && sessions == 1)
                                    dskType = MediaType.CDV;
                            }

                            sense = dev.ReadPma(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                            if(!sense)
                            {
                                if(Decoders.CD.PMA.Decode(cmdBuf).HasValue)
                                {
                                    tmpBuf = new byte[cmdBuf.Length - 4];
                                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                                    sidecar.OpticalDisc[0].PMA = new DumpType();
                                    sidecar.OpticalDisc[0].PMA.Image = outputPrefix + ".pma.bin";
                                    sidecar.OpticalDisc[0].PMA.Size = tmpBuf.Length;
                                    sidecar.OpticalDisc[0].PMA.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PMA.Image, tmpBuf);
                                }
                            }

                            sense = dev.ReadCdText(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                            if(!sense)
                            {
                                if(Decoders.CD.CDTextOnLeadIn.Decode(cmdBuf).HasValue)
                                {
                                    tmpBuf = new byte[cmdBuf.Length - 4];
                                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                                    sidecar.OpticalDisc[0].LeadInCdText = new DumpType();
                                    sidecar.OpticalDisc[0].LeadInCdText.Image = outputPrefix + ".cdtext.bin";
                                    sidecar.OpticalDisc[0].LeadInCdText.Size = tmpBuf.Length;
                                    sidecar.OpticalDisc[0].LeadInCdText.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].LeadInCdText.Image, tmpBuf);
                                }
                            }
                        }
                    }

                    physicalBlockSize = 2448;
                }
                #endregion CompactDisc
                else
                {
                    #region Nintendo
                    if(dskType == MediaType.Unknown && blocks > 0)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            Decoders.DVD.PFI.PhysicalFormatInformation? nintendoPfi = Decoders.DVD.PFI.Decode(cmdBuf);
                            if(nintendoPfi != null)
                            {
                                if(nintendoPfi.Value.DiskCategory == Decoders.DVD.DiskCategory.Nintendo &&
                                    nintendoPfi.Value.PartVersion == 15)
                                {
                                    throw new NotImplementedException("Dumping Nintendo GameCube or Wii discs is not yet implemented.");
                                }
                            }
                        }
                    }
                    #endregion Nintendo

                    #region All DVD and HD DVD types
                    if(dskType == MediaType.DVDDownload || dskType == MediaType.DVDPR ||
                        dskType == MediaType.DVDPRDL || dskType == MediaType.DVDPRW ||
                        dskType == MediaType.DVDPRWDL || dskType == MediaType.DVDR ||
                        dskType == MediaType.DVDRAM || dskType == MediaType.DVDRDL ||
                        dskType == MediaType.DVDROM || dskType == MediaType.DVDRW ||
                        dskType == MediaType.DVDRWDL || dskType == MediaType.HDDVDR ||
                        dskType == MediaType.HDDVDRAM || dskType == MediaType.HDDVDRDL ||
                        dskType == MediaType.HDDVDROM || dskType == MediaType.HDDVDRW ||
                        dskType == MediaType.HDDVDRWDL)
                    {

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            if(Decoders.DVD.PFI.Decode(cmdBuf).HasValue)
                            {
                                tmpBuf = new byte[cmdBuf.Length - 4];
                                Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                                sidecar.OpticalDisc[0].PFI = new DumpType();
                                sidecar.OpticalDisc[0].PFI.Image = outputPrefix + ".pfi.bin";
                                sidecar.OpticalDisc[0].PFI.Size = tmpBuf.Length;
                                sidecar.OpticalDisc[0].PFI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                                DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PFI.Image, tmpBuf);

                                Decoders.DVD.PFI.PhysicalFormatInformation decPfi = Decoders.DVD.PFI.Decode(cmdBuf).Value;
                                DicConsole.WriteLine("PFI:\n{0}", Decoders.DVD.PFI.Prettify(decPfi));

                                // False book types
                                if(dskType == MediaType.DVDROM)
                                {
                                    switch(decPfi.DiskCategory)
                                    {
                                        case Decoders.DVD.DiskCategory.DVDPR:
                                            dskType = MediaType.DVDPR;
                                            break;
                                        case Decoders.DVD.DiskCategory.DVDPRDL:
                                            dskType = MediaType.DVDPRDL;
                                            break;
                                        case Decoders.DVD.DiskCategory.DVDPRW:
                                            dskType = MediaType.DVDPRW;
                                            break;
                                        case Decoders.DVD.DiskCategory.DVDPRWDL:
                                            dskType = MediaType.DVDPRWDL;
                                            break;
                                        case Decoders.DVD.DiskCategory.DVDR:
                                            if(decPfi.PartVersion == 6)
                                                dskType = MediaType.DVDRDL;
                                            else
                                                dskType = MediaType.DVDR;
                                            break;
                                        case Decoders.DVD.DiskCategory.DVDRAM:
                                            dskType = MediaType.DVDRAM;
                                            break;
                                        default:
                                            dskType = MediaType.DVDROM;
                                            break;
                                        case Decoders.DVD.DiskCategory.DVDRW:
                                            if(decPfi.PartVersion == 3)
                                                dskType = MediaType.DVDRWDL;
                                            else
                                                dskType = MediaType.DVDRW;
                                            break;
                                        case Decoders.DVD.DiskCategory.HDDVDR:
                                            dskType = MediaType.HDDVDR;
                                            break;
                                        case Decoders.DVD.DiskCategory.HDDVDRAM:
                                            dskType = MediaType.HDDVDRAM;
                                            break;
                                        case Decoders.DVD.DiskCategory.HDDVDROM:
                                            dskType = MediaType.HDDVDROM;
                                            break;
                                        case Decoders.DVD.DiskCategory.HDDVDRW:
                                            dskType = MediaType.HDDVDRW;
                                            break;
                                        case Decoders.DVD.DiskCategory.Nintendo:
                                            if(decPfi.DiscSize == Decoders.DVD.DVDSize.Eighty)
                                                dskType = MediaType.GOD;
                                            else
                                                dskType = MediaType.WOD;
                                            break;
                                        case Decoders.DVD.DiskCategory.UMD:
                                            dskType = MediaType.UMD;
                                            break;
                                    }
                                }
                            }
                        }
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DiscManufacturingInformation, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            if(Decoders.Xbox.DMI.IsXbox(cmdBuf) || Decoders.Xbox.DMI.IsXbox360(cmdBuf))
                            {
                                if(Decoders.Xbox.DMI.IsXbox(cmdBuf))
                                    dskType = MediaType.XGD;
                                else if(Decoders.Xbox.DMI.IsXbox360(cmdBuf))
                                {
                                    dskType = MediaType.XGD2;

                                    // All XGD3 all have the same number of blocks
                                    if(blocks == 25063 || // Locked (or non compatible drive)
                                       blocks == 4229664 || // Xtreme unlock
                                       blocks == 4246304) // Wxripper unlock
                                        dskType = MediaType.XGD3;
                                }

                                byte[] inqBuf;
                                sense = dev.ScsiInquiry(out inqBuf, out senseBuf);

                                if(sense || !Decoders.SCSI.Inquiry.Decode(inqBuf).HasValue ||
                                   (Decoders.SCSI.Inquiry.Decode(inqBuf).HasValue && !Decoders.SCSI.Inquiry.Decode(inqBuf).Value.KreonPresent))
                                    throw new NotImplementedException("Dumping Xbox Game Discs requires a drive with Kreon firmware.");

                                if(dumpRaw && !force)
                                {
                                    DicConsole.ErrorWriteLine("Not continuing. If you want to continue reading cooked data when raw is not available use the force option.");
                                    // TODO: Exit more gracefully
                                    return;
                                }

                                isXbox = true;
                            }

                            if(cmdBuf.Length == 2052)
                            {
                                tmpBuf = new byte[cmdBuf.Length - 4];
                                Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                                sidecar.OpticalDisc[0].DMI = new DumpType();
                                sidecar.OpticalDisc[0].DMI.Image = outputPrefix + ".dmi.bin";
                                sidecar.OpticalDisc[0].DMI.Size = tmpBuf.Length;
                                sidecar.OpticalDisc[0].DMI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                                DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DMI.Image, tmpBuf);
                            }
                        }
                    }
                    #endregion All DVD and HD DVD types

                    #region DVD-ROM
                    if(dskType == MediaType.DVDDownload || dskType == MediaType.DVDROM)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.CopyrightInformation, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            if(Decoders.DVD.CSS_CPRM.DecodeLeadInCopyright(cmdBuf).HasValue)
                            {
                                tmpBuf = new byte[cmdBuf.Length - 4];
                                Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                                sidecar.OpticalDisc[0].CMI = new DumpType();
                                sidecar.OpticalDisc[0].CMI.Image = outputPrefix + ".cmi.bin";
                                sidecar.OpticalDisc[0].CMI.Size = tmpBuf.Length;
                                sidecar.OpticalDisc[0].CMI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                                DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].CMI.Image, tmpBuf);

                                Decoders.DVD.CSS_CPRM.LeadInCopyright cpy = Decoders.DVD.CSS_CPRM.DecodeLeadInCopyright(cmdBuf).Value;
                                if(cpy.CopyrightType != Decoders.DVD.CopyrightType.NoProtection)
                                    sidecar.OpticalDisc[0].CopyProtection = cpy.CopyrightType.ToString();
                            }
                        }
                    }
                    #endregion DVD-ROM

                    #region DVD-ROM and HD DVD-ROM
                    if(dskType == MediaType.DVDDownload || dskType == MediaType.DVDROM ||
                        dskType == MediaType.HDDVDROM)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.BurstCuttingArea, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].BCA = new DumpType();
                            sidecar.OpticalDisc[0].BCA.Image = outputPrefix + ".bca.bin";
                            sidecar.OpticalDisc[0].BCA.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].BCA.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].BCA.Image, tmpBuf);
                        }
                    }
                    #endregion DVD-ROM and HD DVD-ROM

                    #region DVD-RAM and HD DVD-RAM
                    if(dskType == MediaType.DVDRAM || dskType == MediaType.HDDVDRAM)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDRAM_DDS, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            if(Decoders.DVD.DDS.Decode(cmdBuf).HasValue)
                            {
                                tmpBuf = new byte[cmdBuf.Length - 4];
                                Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                                sidecar.OpticalDisc[0].DDS = new DumpType();
                                sidecar.OpticalDisc[0].DDS.Image = outputPrefix + ".dds.bin";
                                sidecar.OpticalDisc[0].DDS.Size = tmpBuf.Length;
                                sidecar.OpticalDisc[0].DDS.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                                DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DDS.Image, tmpBuf);
                            }
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDRAM_SpareAreaInformation, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            if(Decoders.DVD.Spare.Decode(cmdBuf).HasValue)
                            {
                                tmpBuf = new byte[cmdBuf.Length - 4];
                                Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                                sidecar.OpticalDisc[0].SAI = new DumpType();
                                sidecar.OpticalDisc[0].SAI.Image = outputPrefix + ".sai.bin";
                                sidecar.OpticalDisc[0].SAI.Size = tmpBuf.Length;
                                sidecar.OpticalDisc[0].SAI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                                DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].SAI.Image, tmpBuf);
                            }
                        }
                    }
                    #endregion DVD-RAM and HD DVD-RAM

                    #region DVD-R and DVD-RW
                    if(dskType == MediaType.DVDR || dskType == MediaType.DVDRW)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PreRecordedInfo, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].PRI = new DumpType();
                            sidecar.OpticalDisc[0].PRI.Image = outputPrefix + ".pri.bin";
                            sidecar.OpticalDisc[0].PRI.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].PRI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].SAI.Image, tmpBuf);
                        }
                    }
                    #endregion DVD-R and DVD-RW

                    #region DVD-R, DVD-RW and HD DVD-R
                    if(dskType == MediaType.DVDR || dskType == MediaType.DVDRW || dskType == MediaType.HDDVDR)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDR_MediaIdentifier, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].MediaID = new DumpType();
                            sidecar.OpticalDisc[0].MediaID.Image = outputPrefix + ".mid.bin";
                            sidecar.OpticalDisc[0].MediaID.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].MediaID.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].MediaID.Image, tmpBuf);
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDR_PhysicalInformation, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].PFIR = new DumpType();
                            sidecar.OpticalDisc[0].PFIR.Image = outputPrefix + ".pfir.bin";
                            sidecar.OpticalDisc[0].PFIR.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].PFIR.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PFIR.Image, tmpBuf);
                        }
                    }
                    #endregion DVD-R, DVD-RW and HD DVD-R

                    #region All DVD+
                    if(dskType == MediaType.DVDPR || dskType == MediaType.DVDPRDL ||
                        dskType == MediaType.DVDPRW || dskType == MediaType.DVDPRWDL)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.ADIP, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].ADIP = new DumpType();
                            sidecar.OpticalDisc[0].ADIP.Image = outputPrefix + ".adip.bin";
                            sidecar.OpticalDisc[0].ADIP.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].ADIP.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].ADIP.Image, tmpBuf);
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DCB, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].DCB = new DumpType();
                            sidecar.OpticalDisc[0].DCB.Image = outputPrefix + ".dcb.bin";
                            sidecar.OpticalDisc[0].DCB.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].DCB.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DCB.Image, tmpBuf);
                        }
                    }
                    #endregion All DVD+

                    #region HD DVD-ROM
                    if(dskType == MediaType.HDDVDROM)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.HDDVD_CopyrightInformation, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].CMI = new DumpType();
                            sidecar.OpticalDisc[0].CMI.Image = outputPrefix + ".cmi.bin";
                            sidecar.OpticalDisc[0].CMI.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].CMI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].CMI.Image, tmpBuf);
                        }
                    }
                    #endregion HD DVD-ROM

                    #region All Blu-ray
                    if(dskType == MediaType.BDR || dskType == MediaType.BDRE || dskType == MediaType.BDROM ||
                        dskType == MediaType.BDRXL || dskType == MediaType.BDREXL)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.DiscInformation, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            if(Decoders.Bluray.DI.Decode(cmdBuf).HasValue)
                            {
                                tmpBuf = new byte[cmdBuf.Length - 4];
                                Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                                sidecar.OpticalDisc[0].DI = new DumpType();
                                sidecar.OpticalDisc[0].DI.Image = outputPrefix + ".di.bin";
                                sidecar.OpticalDisc[0].DI.Size = tmpBuf.Length;
                                sidecar.OpticalDisc[0].DI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                                DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DI.Image, tmpBuf);
                            }
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.PAC, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].PAC = new DumpType();
                            sidecar.OpticalDisc[0].PAC.Image = outputPrefix + ".pac.bin";
                            sidecar.OpticalDisc[0].PAC.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].PAC.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PAC.Image, tmpBuf);
                        }
                    }
                    #endregion All Blu-ray


                    #region BD-ROM only
                    if(dskType == MediaType.BDROM)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_BurstCuttingArea, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].BCA = new DumpType();
                            sidecar.OpticalDisc[0].BCA.Image = outputPrefix + ".bca.bin";
                            sidecar.OpticalDisc[0].BCA.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].BCA.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].BCA.Image, tmpBuf);
                        }
                    }
                    #endregion BD-ROM only

                    #region Writable Blu-ray only
                    if(dskType == MediaType.BDR || dskType == MediaType.BDRE ||
                        dskType == MediaType.BDRXL || dskType == MediaType.BDREXL)
                    {
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_DDS, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].DDS = new DumpType();
                            sidecar.OpticalDisc[0].DDS.Image = outputPrefix + ".dds.bin";
                            sidecar.OpticalDisc[0].DDS.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].DDS.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DDS.Image, tmpBuf);
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_SpareAreaInformation, 0, dev.Timeout, out duration);
                        if(!sense)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].SAI = new DumpType();
                            sidecar.OpticalDisc[0].SAI.Image = outputPrefix + ".sai.bin";
                            sidecar.OpticalDisc[0].SAI.Size = tmpBuf.Length;
                            sidecar.OpticalDisc[0].SAI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].SAI.Image, tmpBuf);
                        }
                    }
                    #endregion Writable Blu-ray only
                }
            }
            #endregion MultiMediaDevice
            else
            {
                compactDisc = false;
                sidecar.BlockMedia = new BlockMediaType[1];
                sidecar.BlockMedia[0] = new BlockMediaType();

                // All USB flash drives report as removable, even if the media is not removable
                if(!dev.IsRemovable || dev.IsUSB)
                {
                    if(dev.IsUSB)
                    {
                        sidecar.BlockMedia[0].USB = new USBType();
                        sidecar.BlockMedia[0].USB.ProductID = dev.USBProductID;
                        sidecar.BlockMedia[0].USB.VendorID = dev.USBVendorID;
                        sidecar.BlockMedia[0].USB.Descriptors = new DumpType();
                        sidecar.BlockMedia[0].USB.Descriptors.Image = outputPrefix + ".usbdescriptors.bin";
                        sidecar.BlockMedia[0].USB.Descriptors.Size = dev.USBDescriptors.Length;
                        sidecar.BlockMedia[0].USB.Descriptors.Checksums = Checksum.GetChecksums(dev.USBDescriptors).ToArray();
                        DataFile.WriteTo("SCSI Dump", sidecar.BlockMedia[0].USB.Descriptors.Image, dev.USBDescriptors);
                    }

                    if(dev.Type == DeviceType.ATAPI)
                    {
                        Decoders.ATA.AtaErrorRegistersCHS errorRegs;
                        sense = dev.AtapiIdentify(out cmdBuf, out errorRegs);
                        if(!sense)
                        {
                            sidecar.BlockMedia[0].ATA = new ATAType();
                            sidecar.BlockMedia[0].ATA.Identify = new DumpType();
                            sidecar.BlockMedia[0].ATA.Identify.Image = outputPrefix + ".identify.bin";
                            sidecar.BlockMedia[0].ATA.Identify.Size = cmdBuf.Length;
                            sidecar.BlockMedia[0].ATA.Identify.Checksums = Checksum.GetChecksums(cmdBuf).ToArray();
                            DataFile.WriteTo("SCSI Dump", sidecar.BlockMedia[0].ATA.Identify.Image, cmdBuf);
                        }
                    }

                    sense = dev.ScsiInquiry(out cmdBuf, out senseBuf);
                    if(!sense)
                    {
                        sidecar.BlockMedia[0].SCSI = new SCSIType();
                        sidecar.BlockMedia[0].SCSI.Inquiry = new DumpType();
                        sidecar.BlockMedia[0].SCSI.Inquiry.Image = outputPrefix + ".inquiry.bin";
                        sidecar.BlockMedia[0].SCSI.Inquiry.Size = cmdBuf.Length;
                        sidecar.BlockMedia[0].SCSI.Inquiry.Checksums = Checksum.GetChecksums(cmdBuf).ToArray();
                        DataFile.WriteTo("SCSI Dump", sidecar.BlockMedia[0].SCSI.Inquiry.Image, cmdBuf);

                        sense = dev.ScsiInquiry(out cmdBuf, out senseBuf, 0x00);
                        if(!sense)
                        {
                            byte[] pages = Decoders.SCSI.EVPD.DecodePage00(cmdBuf);

                            if(pages != null)
                            {
                                List<EVPDType> evpds = new List<EVPDType>();
                                foreach(byte page in pages)
                                {
                                    sense = dev.ScsiInquiry(out cmdBuf, out senseBuf, page);
                                    if(!sense)
                                    {
                                        EVPDType evpd = new EVPDType();
                                        evpd.Image = string.Format("{0}.evpd_{1:X2}h.bin", outputPrefix, page);
                                        evpd.Checksums = Checksum.GetChecksums(cmdBuf).ToArray();
                                        evpd.Size = cmdBuf.Length;
                                        evpd.Checksums = Checksum.GetChecksums(cmdBuf).ToArray();
                                        DataFile.WriteTo("SCSI Dump", evpd.Image, cmdBuf);
                                        evpds.Add(evpd);
                                    }
                                }

                                if(evpds.Count > 0)
                                    sidecar.BlockMedia[0].SCSI.EVPD = evpds.ToArray();
                            }
                        }

                        sense = dev.ModeSense10(out cmdBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F, 0xFF, 5, out duration);
                        if(!sense || dev.Error)
                        {
                            sense = dev.ModeSense10(out cmdBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out duration);
                        }

                        Decoders.SCSI.Modes.DecodedMode? decMode = null;

                        if(!sense && !dev.Error)
                        {
                            if(Decoders.SCSI.Modes.DecodeMode10(cmdBuf, dev.SCSIType).HasValue)
                            {
                                decMode = Decoders.SCSI.Modes.DecodeMode10(cmdBuf, dev.SCSIType);
                                sidecar.BlockMedia[0].SCSI.ModeSense10 = new DumpType();
                                sidecar.BlockMedia[0].SCSI.ModeSense10.Image = outputPrefix + ".modesense10.bin";
                                sidecar.BlockMedia[0].SCSI.ModeSense10.Size = cmdBuf.Length;
                                sidecar.BlockMedia[0].SCSI.ModeSense10.Checksums = Checksum.GetChecksums(cmdBuf).ToArray();
                                DataFile.WriteTo("SCSI Dump", sidecar.BlockMedia[0].SCSI.ModeSense10.Image, cmdBuf);
                            }
                        }

                        sense = dev.ModeSense6(out cmdBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out duration);
                        if(sense || dev.Error)
                            sense = dev.ModeSense6(out cmdBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out duration);
                        if(sense || dev.Error)
                            sense = dev.ModeSense(out cmdBuf, out senseBuf, 5, out duration);

                        if(!sense && !dev.Error)
                        {
                            if(Decoders.SCSI.Modes.DecodeMode6(cmdBuf, dev.SCSIType).HasValue)
                            {
                                decMode = Decoders.SCSI.Modes.DecodeMode6(cmdBuf, dev.SCSIType);
                                sidecar.BlockMedia[0].SCSI.ModeSense = new DumpType();
                                sidecar.BlockMedia[0].SCSI.ModeSense.Image = outputPrefix + ".modesense.bin";
                                sidecar.BlockMedia[0].SCSI.ModeSense.Size = cmdBuf.Length;
                                sidecar.BlockMedia[0].SCSI.ModeSense.Checksums = Checksum.GetChecksums(cmdBuf).ToArray();
                                DataFile.WriteTo("SCSI Dump", sidecar.BlockMedia[0].SCSI.ModeSense.Image, cmdBuf);
                            }
                        }

                        if(decMode.HasValue)
                        {
                            scsiMediumType = (byte)decMode.Value.Header.MediumType;
                            if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length >= 1)
                                scsiDensityCode = (byte)decMode.Value.Header.BlockDescriptors[0].Density;

                            foreach(Decoders.SCSI.Modes.ModePage modePage in decMode.Value.Pages)
                                containsFloppyPage |= modePage.Page == 0x05;
                        }
                    }
                }
            }

            if(dskType == MediaType.Unknown)
                dskType = MediaTypeFromSCSI.Get((byte)dev.SCSIType, dev.Manufacturer, dev.Model, scsiMediumType, scsiDensityCode, blocks, blockSize);

            if(dskType == MediaType.Unknown && dev.IsUSB && containsFloppyPage)
                dskType = MediaType.FlashDrive;

            DicConsole.WriteLine("Media identified as {0}", dskType);

            byte[] readBuffer;
            uint blocksToRead = 64;

            ulong errored = 0;

            aborted = false;
            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = aborted = true;
            };

            bool readcd = false;

            #region CompactDisc dump
            if(compactDisc)
            {
                if(toc == null)
                {
                    DicConsole.ErrorWriteLine("Error trying to decode TOC...");
                    return;
                }

                if(dumpRaw)
                {
                    throw new NotImplementedException("Raw CD dumping not yet implemented");
                }
                else
                {
                    // TODO: Check subchannel capabilities
                    readcd = !dev.ReadCd(out readBuffer, out senseBuf, 0, 2448, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                        true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout, out duration);

                    if(readcd)
                        DicConsole.WriteLine("Using MMC READ CD command.");
                }

                DicConsole.WriteLine("Trying to read Lead-In...");
                bool gotLeadIn = false;
                int leadInSectorsGood = 0, leadInSectorsTotal = 0;

                dumpFile = new DataFile(outputPrefix + ".leadin.bin");
                dataChk = new Checksum();

                start = DateTime.UtcNow;

                readBuffer = null;

                for(int leadInBlock = -150; leadInBlock < 0; leadInBlock++)
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

                    DicConsole.Write("\rTrying to read lead-in sector {0} ({1:F3} MiB/sec.)", leadInBlock, currentSpeed);

                    sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)leadInBlock, 2448, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                        true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout, out cmdDuration);

                    if(!sense && !dev.Error)
                    {
                        dataChk.Update(readBuffer);
                        dumpFile.Write(readBuffer);
                        gotLeadIn = true;
                        leadInSectorsGood++;
                        leadInSectorsTotal++;
                    }
                    else
                    {
                        if(gotLeadIn)
                        {
                            // Write empty data
                            dataChk.Update(new byte[2448]);
                            dumpFile.Write(new byte[2448]);
                            leadInSectorsTotal++;
                        }
                    }

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                    currentSpeed = ((double)2448 / (double)1048576) / (cmdDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                }

                dumpFile.Close();
                if(leadInSectorsGood > 0)
                {
                    sidecar.OpticalDisc[0].LeadIn = new BorderType[1];
                    sidecar.OpticalDisc[0].LeadIn[0] = new BorderType();
                    sidecar.OpticalDisc[0].LeadIn[0].Image = outputPrefix + ".leadin.bin";
                    sidecar.OpticalDisc[0].LeadIn[0].Checksums = dataChk.End().ToArray();
                    sidecar.OpticalDisc[0].LeadIn[0].Size = leadInSectorsTotal * 2448;
                }
                else
                    File.Delete(outputPrefix + ".leadin.bin");

                DicConsole.WriteLine();
                DicConsole.WriteLine("Got {0} lead-in sectors.", leadInSectorsGood);

                while(true)
                {
                    if(readcd)
                    {
                        sense = dev.ReadCd(out readBuffer, out senseBuf, 0, 2448, blocksToRead, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                            true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout, out duration);
                        if(dev.Error)
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

                dumpFile = new DataFile(outputPrefix + ".bin");
                mhddLog = new MHDDLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
                ibgLog = new IBGLog(outputPrefix + ".ibg", 0x0008);

                start = DateTime.UtcNow;
                for(ulong i = 0; i < blocks; i += blocksToRead)
                {
                    if(aborted)
                        break;

                    double cmdDuration = 0;

                    if((blocks - i) < blocksToRead)
                        blocksToRead = (uint)(blocks - i);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0)
                        maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0)
                        minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

                    if(readcd)
                    {
                        sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)i, 2448, blocksToRead, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                            true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout, out cmdDuration);
                        totalDuration += cmdDuration;
                    }

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
                        dumpFile.Write(new byte[2448 * blocksToRead]);

                        // TODO: Record error on mapfile

                        errored += blocksToRead;
                        unreadableSectors.Add(i);
                        DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                        if(cmdDuration < 500)
                            mhddLog.Write(i, 65535);
                        else
                            mhddLog.Write(i, cmdDuration);

                        ibgLog.Write(i, 0);
                    }

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                    currentSpeed = ((double)2448 * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                }
                DicConsole.WriteLine();
                end = DateTime.UtcNow;
                mhddLog.Close();
#pragma warning disable IDE0004 // Remove Unnecessary Cast
                ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);
#pragma warning restore IDE0004 // Remove Unnecessary Cast

                #region Compact Disc Error handling
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

                cdRepeatRetry:
                    ulong[] tmpArray = unreadableSectors.ToArray();
                    foreach(ulong badSector in tmpArray)
                    {
                        if(aborted)
                            break;

                        double cmdDuration = 0;

                        DicConsole.Write("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass + 1, forward ? "forward" : "reverse", runningPersistent ? "recovering partial data, " : "");

                        if(readcd)
                        {
                            sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)badSector, 2448, blocksToRead, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout, out cmdDuration);
                            totalDuration += cmdDuration;
                        }

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
                        goto cdRepeatRetry;
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

                        Decoders.SCSI.Modes.ModePage_01_MMC pgMMC = new Decoders.SCSI.Modes.ModePage_01_MMC();
                        pgMMC.PS = false;
                        pgMMC.ReadRetryCount = 255;
                        pgMMC.Parameter = 0x20;

                        Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode();
                        md.Header = new Decoders.SCSI.Modes.ModeHeader();
                        md.Pages = new Decoders.SCSI.Modes.ModePage[1];
                        md.Pages[0] = new Decoders.SCSI.Modes.ModePage();
                        md.Pages[0].Page = 0x01;
                        md.Pages[0].Subpage = 0x00;
                        md.Pages[0].PageResponse = Decoders.SCSI.Modes.EncodeModePage_01_MMC(pgMMC);
                        md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.SCSIType);
                        md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.SCSIType);

                        sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out duration);
                        if(sense)
                        {
                            sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out duration);
                        }

                        runningPersistent = true;
                        if(!sense && !dev.Error)
                        {
                            pass--;
                            goto cdRepeatRetry;
                        }
                    }
                    else if(runningPersistent && persistent && currentModePage.HasValue)
                    {
                        Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode();
                        md.Header = new Decoders.SCSI.Modes.ModeHeader();
                        md.Pages = new Decoders.SCSI.Modes.ModePage[1];
                        md.Pages[0] = currentModePage.Value;
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
                #endregion Compact Disc Error handling

                dataChk = new Checksum();
                dumpFile.Seek(0, SeekOrigin.Begin);
                blocksToRead = 500;

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

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                    currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (chkDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                }
                DicConsole.WriteLine();
                dumpFile.Close();
                end = DateTime.UtcNow;

                // TODO: Correct this
                sidecar.OpticalDisc[0].Checksums = dataChk.End().ToArray();
                sidecar.OpticalDisc[0].DumpHardwareArray = new DumpHardwareType[1];
                sidecar.OpticalDisc[0].DumpHardwareArray[0] = new DumpHardwareType();
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents = new ExtentType[1];
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0] = new ExtentType();
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0].Start = 0;
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0].End = (int)(blocks - 1);
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Manufacturer = dev.Manufacturer;
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Model = dev.Model;
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Revision = dev.Revision;
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Software = new SoftwareType();
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.Name = "DiscImageChef";
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.OperatingSystem = dev.PlatformID.ToString();
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.Version = typeof(SCSI).Assembly.GetName().Version.ToString();
                sidecar.OpticalDisc[0].Image = new ImageType();
                sidecar.OpticalDisc[0].Image.format = "Raw disk image (sector by sector copy)";
                sidecar.OpticalDisc[0].Image.Value = outputPrefix + ".bin";
                sidecar.OpticalDisc[0].Sessions = 1;
                sidecar.OpticalDisc[0].Tracks = new[] { 1 };
                sidecar.OpticalDisc[0].Track = new Schemas.TrackType[1];
                sidecar.OpticalDisc[0].Track[0] = new Schemas.TrackType();
                sidecar.OpticalDisc[0].Track[0].BytesPerSector = (int)blockSize;
                sidecar.OpticalDisc[0].Track[0].Checksums = sidecar.OpticalDisc[0].Checksums;
                sidecar.OpticalDisc[0].Track[0].EndSector = (long)(blocks - 1);
                sidecar.OpticalDisc[0].Track[0].Image = new ImageType();
                sidecar.OpticalDisc[0].Track[0].Image.format = "BINARY";
                sidecar.OpticalDisc[0].Track[0].Image.offset = 0;
                sidecar.OpticalDisc[0].Track[0].Image.offsetSpecified = true;
                sidecar.OpticalDisc[0].Track[0].Image.Value = sidecar.OpticalDisc[0].Image.Value;
                sidecar.OpticalDisc[0].Track[0].Sequence = new TrackSequenceType();
                sidecar.OpticalDisc[0].Track[0].Sequence.Session = 1;
                sidecar.OpticalDisc[0].Track[0].Sequence.TrackNumber = 1;
                sidecar.OpticalDisc[0].Track[0].Size = (long)(blocks * blockSize);
                sidecar.OpticalDisc[0].Track[0].StartSector = 0;
                sidecar.OpticalDisc[0].Track[0].TrackType1 = TrackTypeTrackType.mode1;
                sidecar.OpticalDisc[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(dskType);
                string xmlDskTyp, xmlDskSubTyp;
                Metadata.MediaType.MediaTypeToString(dskType, out xmlDskTyp, out xmlDskSubTyp);
                sidecar.OpticalDisc[0].DiscType = xmlDskTyp;
                sidecar.OpticalDisc[0].DiscSubType = xmlDskSubTyp;
            }
            #endregion CompactDisc dump
            #region Xbox Game Disc
            else if(isXbox)
            {
                byte[] ssBuf;
                sense = dev.KreonExtractSS(out ssBuf, out senseBuf, dev.Timeout, out duration);
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
                DataFile.WriteTo("SCSI Dump", outputPrefix + ".ss.bin", cmdBuf);

                ulong l0Video, l1Video, middleZone, gameSize, totalSize, layerBreak;

                // Get video partition size
                DicConsole.DebugWriteLine("Dump-media command", "Getting video partition size");
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
                        if(dev.Error)
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
                ibgLog = new IBGLog(outputPrefix + ".ibg", currentProfile);
                dumpFile = new DataFile(outputPrefix + ".bin");

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
                            unreadableSectors.Add(i);
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
                            Decoders.SCSI.Modes.ModePage_01_MMC pgMMC = new Decoders.SCSI.Modes.ModePage_01_MMC();
                            pgMMC.PS = false;
                            pgMMC.ReadRetryCount = 255;
                            pgMMC.Parameter = 0x20;

                            Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode();
                            md.Header = new Decoders.SCSI.Modes.ModeHeader();
                            md.Pages = new Decoders.SCSI.Modes.ModePage[1];
                            md.Pages[0] = new Decoders.SCSI.Modes.ModePage();
                            md.Pages[0].Page = 0x01;
                            md.Pages[0].Subpage = 0x00;
                            md.Pages[0].PageResponse = Decoders.SCSI.Modes.EncodeModePage_01_MMC(pgMMC);
                            md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.SCSIType);
                            md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.SCSIType);
                        }
                        else
                        {
                            Decoders.SCSI.Modes.ModePage_01 pg = new Decoders.SCSI.Modes.ModePage_01();
                            pg.PS = false;
                            pg.AWRE = false;
                            pg.ARRE = false;
                            pg.TB = true;
                            pg.RC = false;
                            pg.EER = true;
                            pg.PER = false;
                            pg.DTE = false;
                            pg.DCR = false;
                            pg.ReadRetryCount = 255;

                            Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode();
                            md.Header = new Decoders.SCSI.Modes.ModeHeader();
                            md.Pages = new Decoders.SCSI.Modes.ModePage[1];
                            md.Pages[0] = new Decoders.SCSI.Modes.ModePage();
                            md.Pages[0].Page = 0x01;
                            md.Pages[0].Subpage = 0x00;
                            md.Pages[0].PageResponse = Decoders.SCSI.Modes.EncodeModePage_01(pg);
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
                        Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode();
                        md.Header = new Decoders.SCSI.Modes.ModeHeader();
                        md.Pages = new Decoders.SCSI.Modes.ModePage[1];
                        md.Pages[0] = currentModePage.Value;
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
                Filter inputFilter = filtersList.GetFilter(outputPrefix + ".bin");

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
                        List<Partition> _partitions;

                        if(_partplugin.GetInformation(_imageFormat, out _partitions))
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
                            xmlFileSysInfo[i] = new PartitionType();
                            xmlFileSysInfo[i].Description = partitions[i].PartitionDescription;
                            xmlFileSysInfo[i].EndSector = (int)(partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1);
                            xmlFileSysInfo[i].Name = partitions[i].PartitionName;
                            xmlFileSysInfo[i].Sequence = (int)partitions[i].PartitionSequence;
                            xmlFileSysInfo[i].StartSector = (int)partitions[i].PartitionStartSector;
                            xmlFileSysInfo[i].Type = partitions[i].PartitionType;

                            List<FileSystemType> lstFs = new List<FileSystemType>();

                            foreach(Filesystem _plugin in plugins.PluginsList.Values)
                            {
                                try
                                {
                                    if(_plugin.Identify(_imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1))
                                    {
                                        string foo;
                                        _plugin.GetInformation(_imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1, out foo);
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
                        xmlFileSysInfo[0] = new PartitionType();
                        xmlFileSysInfo[0].EndSector = (int)(blocks - 1);
                        xmlFileSysInfo[0].StartSector = 0;

                        List<FileSystemType> lstFs = new List<FileSystemType>();

                        foreach(Filesystem _plugin in plugins.PluginsList.Values)
                        {
                            try
                            {
                                if(_plugin.Identify(_imageFormat, (blocks - 1), 0))
                                {
                                    string foo;
                                    _plugin.GetInformation(_imageFormat, (blocks - 1), 0, out foo);
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
                sidecar.OpticalDisc[0].DumpHardwareArray[0] = new DumpHardwareType();
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents = new ExtentType[1];
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0] = new ExtentType();
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0].Start = 0;
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0].End = (int)(blocks - 1);
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Manufacturer = dev.Manufacturer;
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Model = dev.Model;
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Revision = dev.Revision;
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Software = new SoftwareType();
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.Name = "DiscImageChef";
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.OperatingSystem = dev.PlatformID.ToString();
                sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.Version = typeof(SCSI).Assembly.GetName().Version.ToString();
                sidecar.OpticalDisc[0].Image = new ImageType();
                sidecar.OpticalDisc[0].Image.format = "Raw disk image (sector by sector copy)";
                sidecar.OpticalDisc[0].Image.Value = outputPrefix + ".bin";
                sidecar.OpticalDisc[0].Layers = new LayersType();
                sidecar.OpticalDisc[0].Layers.type = LayersTypeType.OTP;
                sidecar.OpticalDisc[0].Layers.typeSpecified = true;
                sidecar.OpticalDisc[0].Layers.Sectors = new SectorsType[1];
                sidecar.OpticalDisc[0].Layers.Sectors[0] = new SectorsType();
                sidecar.OpticalDisc[0].Layers.Sectors[0].Value = (long)layerBreak;
                sidecar.OpticalDisc[0].Sessions = 1;
                sidecar.OpticalDisc[0].Tracks = new[] { 1 };
                sidecar.OpticalDisc[0].Track = new Schemas.TrackType[1];
                sidecar.OpticalDisc[0].Track[0] = new Schemas.TrackType();
                sidecar.OpticalDisc[0].Track[0].BytesPerSector = (int)blockSize;
                sidecar.OpticalDisc[0].Track[0].Checksums = sidecar.OpticalDisc[0].Checksums;
                sidecar.OpticalDisc[0].Track[0].EndSector = (long)(blocks - 1);
                sidecar.OpticalDisc[0].Track[0].Image = new ImageType();
                sidecar.OpticalDisc[0].Track[0].Image.format = "BINARY";
                sidecar.OpticalDisc[0].Track[0].Image.offset = 0;
                sidecar.OpticalDisc[0].Track[0].Image.offsetSpecified = true;
                sidecar.OpticalDisc[0].Track[0].Image.Value = sidecar.OpticalDisc[0].Image.Value;
                sidecar.OpticalDisc[0].Track[0].Sequence = new TrackSequenceType();
                sidecar.OpticalDisc[0].Track[0].Sequence.Session = 1;
                sidecar.OpticalDisc[0].Track[0].Sequence.TrackNumber = 1;
                sidecar.OpticalDisc[0].Track[0].Size = (long)(totalSize * blockSize);
                sidecar.OpticalDisc[0].Track[0].StartSector = 0;
                if(xmlFileSysInfo != null)
                    sidecar.OpticalDisc[0].Track[0].FileSystemInformation = xmlFileSysInfo;
                sidecar.OpticalDisc[0].Track[0].TrackType1 = TrackTypeTrackType.dvd;
                sidecar.OpticalDisc[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(dskType);
                string xmlDskTyp, xmlDskSubTyp;
                Metadata.MediaType.MediaTypeToString(dskType, out xmlDskTyp, out xmlDskSubTyp);
                sidecar.OpticalDisc[0].DiscType = xmlDskTyp;
                sidecar.OpticalDisc[0].DiscSubType = xmlDskSubTyp;
            }
            #endregion Xbox Game Disc
            else
            {
                uint longBlockSize = scsiReader.LongBlockSize;

                if(dumpRaw)
                {
                    if(blockSize == longBlockSize)
                    {
                        if(!scsiReader.CanReadRaw)
                        {
                            DicConsole.ErrorWriteLine("Device doesn't seem capable of reading raw data from media.");
                        }
                        else
                        {
                            DicConsole.ErrorWriteLine("Device is capable of reading raw data but I've been unable to guess correct sector size.");
                        }

                        if(!force)
                        {
                            DicConsole.ErrorWriteLine("Not continuing. If you want to continue reading cooked data when raw is not available use the force option.");
                            // TODO: Exit more gracefully
                            return;
                        }

                        DicConsole.ErrorWriteLine("Continuing dumping cooked data.");
                        dumpRaw = false;
                    }
                    else
                    {

                        if(longBlockSize == 37856) // Only a block will be read, but it contains 16 sectors and command expect sector number not block number
                            blocksToRead = 16;
                        else
                            blocksToRead = 1;
                        DicConsole.WriteLine("Reading {0} raw bytes ({1} cooked bytes) per sector.",
                                             longBlockSize, blockSize * blocksToRead);
                        physicalBlockSize = longBlockSize;
                        blockSize = longBlockSize;
                    }

                }
                DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

                mhddLog = new MHDDLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
                ibgLog = new IBGLog(outputPrefix + ".ibg", currentProfile);
                dumpFile = new DataFile(outputPrefix + ".bin");

                start = DateTime.UtcNow;

                readBuffer = null;

                for(ulong i = 0; i < blocks; i += blocksToRead)
                {
                    if(aborted)
                        break;

                    double cmdDuration = 0;

                    if((blocks - i) < blocksToRead)
                        blocksToRead = (uint)(blocks - i);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0)
                        maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0)
                        minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

                    sense = scsiReader.ReadBlocks(out readBuffer, i, blocksToRead, out cmdDuration);
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
                        unreadableSectors.Add(i);
                        if(cmdDuration < 500)
                            mhddLog.Write(i, 65535);
                        else
                            mhddLog.Write(i, cmdDuration);

                        ibgLog.Write(i, 0);
                    }

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                    currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
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

                        double cmdDuration = 0;

                        DicConsole.Write("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass + 1, forward ? "forward" : "reverse", runningPersistent ? "recovering partial data, " : "");

                        sense = scsiReader.ReadBlock(out readBuffer, badSector, out cmdDuration);
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
                            Decoders.SCSI.Modes.ModePage_01_MMC pgMMC = new Decoders.SCSI.Modes.ModePage_01_MMC();
                            pgMMC.PS = false;
                            pgMMC.ReadRetryCount = 255;
                            pgMMC.Parameter = 0x20;

                            Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode();
                            md.Header = new Decoders.SCSI.Modes.ModeHeader();
                            md.Pages = new Decoders.SCSI.Modes.ModePage[1];
                            md.Pages[0] = new Decoders.SCSI.Modes.ModePage();
                            md.Pages[0].Page = 0x01;
                            md.Pages[0].Subpage = 0x00;
                            md.Pages[0].PageResponse = Decoders.SCSI.Modes.EncodeModePage_01_MMC(pgMMC);
                            md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.SCSIType);
                            md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.SCSIType);
                        }
                        else
                        {
                            Decoders.SCSI.Modes.ModePage_01 pg = new Decoders.SCSI.Modes.ModePage_01();
                            pg.PS = false;
                            pg.AWRE = false;
                            pg.ARRE = false;
                            pg.TB = true;
                            pg.RC = false;
                            pg.EER = true;
                            pg.PER = false;
                            pg.DTE = false;
                            pg.DCR = false;
                            pg.ReadRetryCount = 255;

                            Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode();
                            md.Header = new Decoders.SCSI.Modes.ModeHeader();
                            md.Pages = new Decoders.SCSI.Modes.ModePage[1];
                            md.Pages[0] = new Decoders.SCSI.Modes.ModePage();
                            md.Pages[0].Page = 0x01;
                            md.Pages[0].Subpage = 0x00;
                            md.Pages[0].PageResponse = Decoders.SCSI.Modes.EncodeModePage_01(pg);
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
                        Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode();
                        md.Header = new Decoders.SCSI.Modes.ModeHeader();
                        md.Pages = new Decoders.SCSI.Modes.ModePage[1];
                        md.Pages[0] = currentModePage.Value;
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
                Filter inputFilter = filtersList.GetFilter(outputPrefix + ".bin");

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
                        List<Partition> _partitions;

                        if(_partplugin.GetInformation(_imageFormat, out _partitions))
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
                            xmlFileSysInfo[i] = new PartitionType();
                            xmlFileSysInfo[i].Description = partitions[i].PartitionDescription;
                            xmlFileSysInfo[i].EndSector = (int)(partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1);
                            xmlFileSysInfo[i].Name = partitions[i].PartitionName;
                            xmlFileSysInfo[i].Sequence = (int)partitions[i].PartitionSequence;
                            xmlFileSysInfo[i].StartSector = (int)partitions[i].PartitionStartSector;
                            xmlFileSysInfo[i].Type = partitions[i].PartitionType;

                            List<FileSystemType> lstFs = new List<FileSystemType>();

                            foreach(Filesystem _plugin in plugins.PluginsList.Values)
                            {
                                try
                                {
                                    if(_plugin.Identify(_imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1))
                                    {
                                        string foo;
                                        _plugin.GetInformation(_imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1, out foo);
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
                        xmlFileSysInfo[0] = new PartitionType();
                        xmlFileSysInfo[0].EndSector = (int)(blocks - 1);
                        xmlFileSysInfo[0].StartSector = 0;

                        List<FileSystemType> lstFs = new List<FileSystemType>();

                        foreach(Filesystem _plugin in plugins.PluginsList.Values)
                        {
                            try
                            {
                                if(_plugin.Identify(_imageFormat, (blocks - 1), 0))
                                {
                                    string foo;
                                    _plugin.GetInformation(_imageFormat, (blocks - 1), 0, out foo);
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

                if(opticalDisc)
                {
                    sidecar.OpticalDisc[0].Checksums = dataChk.End().ToArray();
                    sidecar.OpticalDisc[0].DumpHardwareArray = new DumpHardwareType[1];
                    sidecar.OpticalDisc[0].DumpHardwareArray[0] = new DumpHardwareType();
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents = new ExtentType[1];
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0] = new ExtentType();
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0].Start = 0;
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0].End = (int)(blocks - 1);
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Manufacturer = dev.Manufacturer;
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Model = dev.Model;
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Revision = dev.Revision;
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Software = new SoftwareType();
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.Name = "DiscImageChef";
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.OperatingSystem = dev.PlatformID.ToString();
                    sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.Version = typeof(SCSI).Assembly.GetName().Version.ToString();
                    sidecar.OpticalDisc[0].Image = new ImageType();
                    sidecar.OpticalDisc[0].Image.format = "Raw disk image (sector by sector copy)";
                    sidecar.OpticalDisc[0].Image.Value = outputPrefix + ".bin";
                    // TODO: Implement layers
                    //sidecar.OpticalDisc[0].Layers = new LayersType();
                    sidecar.OpticalDisc[0].Sessions = 1;
                    sidecar.OpticalDisc[0].Tracks = new[] { 1 };
                    sidecar.OpticalDisc[0].Track = new Schemas.TrackType[1];
                    sidecar.OpticalDisc[0].Track[0] = new Schemas.TrackType();
                    sidecar.OpticalDisc[0].Track[0].BytesPerSector = (int)blockSize;
                    sidecar.OpticalDisc[0].Track[0].Checksums = sidecar.OpticalDisc[0].Checksums;
                    sidecar.OpticalDisc[0].Track[0].EndSector = (long)(blocks - 1);
                    sidecar.OpticalDisc[0].Track[0].Image = new ImageType();
                    sidecar.OpticalDisc[0].Track[0].Image.format = "BINARY";
                    sidecar.OpticalDisc[0].Track[0].Image.offset = 0;
                    sidecar.OpticalDisc[0].Track[0].Image.offsetSpecified = true;
                    sidecar.OpticalDisc[0].Track[0].Image.Value = sidecar.OpticalDisc[0].Image.Value;
                    sidecar.OpticalDisc[0].Track[0].Sequence = new TrackSequenceType();
                    sidecar.OpticalDisc[0].Track[0].Sequence.Session = 1;
                    sidecar.OpticalDisc[0].Track[0].Sequence.TrackNumber = 1;
                    sidecar.OpticalDisc[0].Track[0].Size = (long)(blocks * blockSize);
                    sidecar.OpticalDisc[0].Track[0].StartSector = 0;
                    if(xmlFileSysInfo != null)
                        sidecar.OpticalDisc[0].Track[0].FileSystemInformation = xmlFileSysInfo;
                    switch(dskType)
                    {
                        case MediaType.DDCD:
                        case MediaType.DDCDR:
                        case MediaType.DDCDRW:
                            sidecar.OpticalDisc[0].Track[0].TrackType1 = TrackTypeTrackType.ddcd;
                            break;
                        case MediaType.DVDROM:
                        case MediaType.DVDR:
                        case MediaType.DVDRAM:
                        case MediaType.DVDRW:
                        case MediaType.DVDRDL:
                        case MediaType.DVDRWDL:
                        case MediaType.DVDDownload:
                        case MediaType.DVDPRW:
                        case MediaType.DVDPR:
                        case MediaType.DVDPRWDL:
                        case MediaType.DVDPRDL:
                            sidecar.OpticalDisc[0].Track[0].TrackType1 = TrackTypeTrackType.dvd;
                            break;
                        case MediaType.HDDVDROM:
                        case MediaType.HDDVDR:
                        case MediaType.HDDVDRAM:
                        case MediaType.HDDVDRW:
                        case MediaType.HDDVDRDL:
                        case MediaType.HDDVDRWDL:
                            sidecar.OpticalDisc[0].Track[0].TrackType1 = TrackTypeTrackType.hddvd;
                            break;
                        case MediaType.BDROM:
                        case MediaType.BDR:
                        case MediaType.BDRE:
                        case MediaType.BDREXL:
                        case MediaType.BDRXL:
                            sidecar.OpticalDisc[0].Track[0].TrackType1 = TrackTypeTrackType.bluray;
                            break;
                    }
                    sidecar.OpticalDisc[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(dskType);
                    string xmlDskTyp, xmlDskSubTyp;
                    Metadata.MediaType.MediaTypeToString(dskType, out xmlDskTyp, out xmlDskSubTyp);
                    sidecar.OpticalDisc[0].DiscType = xmlDskTyp;
                    sidecar.OpticalDisc[0].DiscSubType = xmlDskSubTyp;
                }
                else
                {
                    sidecar.BlockMedia[0].Checksums = dataChk.End().ToArray();
                    sidecar.BlockMedia[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(dskType);
                    string xmlDskTyp, xmlDskSubTyp;
                    Metadata.MediaType.MediaTypeToString(dskType, out xmlDskTyp, out xmlDskSubTyp);
                    sidecar.BlockMedia[0].DiskType = xmlDskTyp;
                    sidecar.BlockMedia[0].DiskSubType = xmlDskSubTyp;
                    // TODO: Implement device firmware revision
                    sidecar.BlockMedia[0].Image = new ImageType();
                    sidecar.BlockMedia[0].Image.format = "Raw disk image (sector by sector copy)";
                    sidecar.BlockMedia[0].Image.Value = outputPrefix + ".bin";
                    if(!dev.IsRemovable || dev.IsUSB)
                    {
                        if(dev.Type == DeviceType.ATAPI)
                            sidecar.BlockMedia[0].Interface = "ATAPI";
                        else if(dev.IsUSB)
                            sidecar.BlockMedia[0].Interface = "USB";
                        else if(dev.IsFireWire)
                            sidecar.BlockMedia[0].Interface = "FireWire";
                        else
                            sidecar.BlockMedia[0].Interface = "SCSI";
                    }
                    sidecar.BlockMedia[0].LogicalBlocks = (long)blocks;
                    sidecar.BlockMedia[0].PhysicalBlockSize = (int)physicalBlockSize;
                    sidecar.BlockMedia[0].LogicalBlockSize = (int)logicalBlockSize;
                    sidecar.BlockMedia[0].Manufacturer = dev.Manufacturer;
                    sidecar.BlockMedia[0].Model = dev.Model;
                    sidecar.BlockMedia[0].Serial = dev.Serial;
                    sidecar.BlockMedia[0].Size = (long)(blocks * blockSize);
                    if(xmlFileSysInfo != null)
                        sidecar.BlockMedia[0].FileSystemInformation = xmlFileSysInfo;

                    if(dev.IsRemovable)
                    {
                        sidecar.BlockMedia[0].DumpHardwareArray = new DumpHardwareType[1];
                        sidecar.BlockMedia[0].DumpHardwareArray[0] = new DumpHardwareType();
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Extents = new ExtentType[1];
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Extents[0] = new ExtentType();
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Extents[0].Start = 0;
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Extents[0].End = (int)(blocks - 1);
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Manufacturer = dev.Manufacturer;
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Model = dev.Model;
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Revision = dev.Revision;
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Software = new SoftwareType();
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Software.Name = "DiscImageChef";
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Software.OperatingSystem = dev.PlatformID.ToString();
                        sidecar.BlockMedia[0].DumpHardwareArray[0].Software.Version = typeof(SCSI).Assembly.GetName().Version.ToString();
                    }
                }
            }

            DicConsole.WriteLine();

            DicConsole.WriteLine("Took a total of {0:F3} seconds ({1:F3} processing commands, {2:F3} checksumming).", (end - start).TotalSeconds, totalDuration / 1000, totalChkDuration / 1000);
#pragma warning disable IDE0004 // Cast is necessary, otherwise incorrect value is created
            DicConsole.WriteLine("Avegare speed: {0:F3} MiB/sec.", (((double)blockSize * (double)(blocks + 1)) / 1048576) / (totalDuration / 1000));
#pragma warning restore IDE0004 // Cast is necessary, otherwise incorrect value is created
            DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", maxSpeed);
            DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", minSpeed);
            DicConsole.WriteLine("{0} sectors could not be read.", unreadableSectors.Count);
            if(unreadableSectors.Count > 0)
            {
                unreadableSectors.Sort();
                foreach(ulong bad in unreadableSectors)
                    DicConsole.WriteLine("Sector {0} could not be read", bad);
            }
            DicConsole.WriteLine();

            if(!aborted)
            {
                DicConsole.WriteLine("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml",
                                       FileMode.Create);

                System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }

            Statistics.AddMedia(dskType, true);
        }
    }
}
