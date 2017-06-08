// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SSC.cs
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
using Schemas;

namespace DiscImageChef.Core.Devices.Dumping
{
    internal static class SSC
    {
        internal static void Dump(Device dev, string outputPrefix, string devicePath, ref CICMMetadataType sidecar, ref Metadata.Resume resume)
        {
            Decoders.SCSI.FixedSense? fxSense;
            bool aborted;
            MHDDLog mhddLog;
            IBGLog ibgLog;
            bool sense = false;
            ulong blocks = 0;
            uint blockSize = 0;
            MediaType dskType = MediaType.Unknown;
            DateTime start;
            DateTime end;
            double totalDuration = 0;
            double totalChkDuration = 0;
            double currentSpeed = 0;
            double maxSpeed = double.MinValue;
            double minSpeed = double.MaxValue;
            List<ulong> unreadableSectors = new List<ulong>();
            Checksum dataChk;

            dev.RequestSense(out byte[] senseBuf, dev.Timeout, out double duration);
            fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out string strSense);

            if(fxSense.HasValue && fxSense.Value.SenseKey != Decoders.SCSI.SenseKeys.NoSense)
            {
                DicConsole.ErrorWriteLine("Drive has status error, please correct. Sense follows...");
                DicConsole.ErrorWriteLine("{0}", strSense);
                return;
            }

            // Not in BOM/P
            if(fxSense.HasValue && fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ != 0x00 && fxSense.Value.ASCQ != 0x04 && fxSense.Value.SenseKey != Decoders.SCSI.SenseKeys.IllegalRequest)
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
            sense = dev.ReadPosition(out byte[] cmdBuf, out senseBuf, SscPositionForms.Short, dev.Timeout, out duration);

            if(sense)
            {
                // READ POSITION is mandatory starting SCSI-2, so do not cry if the drive does not recognize the command (SCSI-1 or earlier)
                // Anyway, <=SCSI-1 tapes do not support partitions
                fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);

                if(fxSense.HasValue && ((fxSense.Value.ASC == 0x20 && fxSense.Value.ASCQ != 0x00) || fxSense.Value.ASC != 0x20 && fxSense.Value.SenseKey != Decoders.SCSI.SenseKeys.IllegalRequest))
                {
                    DicConsole.ErrorWriteLine("Could not get position. Sense follows...");
                    DicConsole.ErrorWriteLine("{0}", strSense);
                    return;
                }
            }
            else
            {
                // Not in partition 0
                if(cmdBuf[1] != 0)
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

                    sense = dev.ReadPosition(out cmdBuf, out senseBuf, SscPositionForms.Short, dev.Timeout, out duration);
                    if(sense)
                    {
                        fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                        DicConsole.ErrorWriteLine("Drive could not rewind, please correct. Sense follows...");
                        DicConsole.ErrorWriteLine("{0}", strSense);
                        return;
                    }

                    // Still not in partition 0!!!?
                    if(cmdBuf[1] != 0)
                    {
                        DicConsole.ErrorWriteLine("Drive could not rewind to partition 0 but no error occurred...");
                        return;
                    }

                    DicConsole.WriteLine();
                }
            }

            sidecar.BlockMedia = new BlockMediaType[1];
            sidecar.BlockMedia[0] = new BlockMediaType
            {
                SCSI = new SCSIType()
            };
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
                    sidecar.BlockMedia[0].SCSI.ModeSense10 = new DumpType
                    {
                        Image = outputPrefix + ".modesense10.bin",
                        Size = cmdBuf.Length,
                        Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                    };
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
                    sidecar.BlockMedia[0].SCSI.ModeSense = new DumpType
                    {
                        Image = outputPrefix + ".modesense.bin",
                        Size = cmdBuf.Length,
                        Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                    };
                    DataFile.WriteTo("SCSI Dump", sidecar.BlockMedia[0].SCSI.ModeSense.Image, cmdBuf);
                }
            }

            // TODO: Check partitions page
            if(decMode.HasValue)
            {
                scsiMediumTypeTape = (byte)decMode.Value.Header.MediumType;
                if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length >= 1)
                    scsiDensityCodeTape = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                blockSize = decMode.Value.Header.BlockDescriptors[0].BlockLength;
            }
            else
                blockSize = 1;

            if(dskType == MediaType.Unknown)
                dskType = MediaTypeFromSCSI.Get((byte)dev.SCSIType, dev.Manufacturer, dev.Model, scsiMediumTypeTape, scsiDensityCodeTape, blocks, blockSize);

            DicConsole.WriteLine("Media identified as {0}", dskType);

            bool endOfMedia = false;
            ulong currentBlock = 0;
            ulong currentFile = 0;
            byte currentPartition = 0;
            byte totalPartitions = 1; // TODO: Handle partitions.
            ulong currentSize = 0;
            ulong currentPartitionSize = 0;
            ulong currentFileSize = 0;

            bool fixedLen = false;
            uint transferLen = blockSize;

            sense = dev.Read6(out cmdBuf, out senseBuf, false, fixedLen, transferLen, blockSize, dev.Timeout, out duration);
            if(sense)
            {
                fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                if(fxSense.HasValue)
                {
                    if(fxSense.Value.SenseKey == Decoders.SCSI.SenseKeys.IllegalRequest)
                    {
                        sense = dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, dev.Timeout, out duration);
                        if(sense)
                        {
                            fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                            if(!fxSense.HasValue || !fxSense.Value.EOM)
                            {
                                DicConsole.WriteLine();
                                DicConsole.ErrorWriteLine("Drive could not return back. Sense follows...");
                                DicConsole.ErrorWriteLine("{0}", strSense);
                                return;
                            }
                        }

                        fixedLen = true;
                        transferLen = 1;
                        sense = dev.Read6(out cmdBuf, out senseBuf, false, fixedLen, transferLen, blockSize, dev.Timeout, out duration);
                        if(sense)
                        {
                            DicConsole.WriteLine();
                            DicConsole.ErrorWriteLine("Drive could not read. Sense follows...");
                            DicConsole.ErrorWriteLine("{0}", strSense);
                            return;
                        }
                    }
                    else
                    {
                        DicConsole.WriteLine();
                        DicConsole.ErrorWriteLine("Drive could not read. Sense follows...");
                        DicConsole.ErrorWriteLine("{0}", strSense);
                        return;
                    }
                }
                else
                {
                    DicConsole.WriteLine();
                    DicConsole.ErrorWriteLine("Cannot read device, don't know why, exiting...");
                    return;
                }
            }

            sense = dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, dev.Timeout, out duration);
            if(sense)
            {
                fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                if(!fxSense.HasValue || !fxSense.Value.EOM)
                {
                    DicConsole.WriteLine();
                    DicConsole.ErrorWriteLine("Drive could not return back. Sense follows...");
                    DicConsole.ErrorWriteLine("{0}", strSense);
                    return;
                }
            }

            Checksum partitionChk;
            Checksum fileChk;
            List<TapePartitionType> partitions = new List<TapePartitionType>();
            List<TapeFileType> files = new List<TapeFileType>();
            TapeFileType currentTapeFile = new TapeFileType();
            TapePartitionType currentTapePartition = new TapePartitionType();

            DicConsole.WriteLine();
            DataFile dumpFile = new DataFile(outputPrefix + ".bin");
            dataChk = new Checksum();
            start = DateTime.UtcNow;
            mhddLog = new MHDDLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, 1);
            ibgLog = new IBGLog(outputPrefix + ".ibg", 0x0008);

            currentTapeFile = new TapeFileType
            {
                Image = new ImageType
                {
                    format = "BINARY",
                    offset = (long)currentSize,
                    offsetSpecified = true,
                    Value = outputPrefix + ".bin"
                },
                Sequence = (long)currentFile,
                StartBlock = (long)currentBlock,
                BlockSize = blockSize
            };
            fileChk = new Checksum();
            currentTapePartition = new TapePartitionType
            {
                Image = new ImageType
                {
                    format = "BINARY",
                    offset = (long)currentSize,
                    offsetSpecified = true,
                    Value = outputPrefix + ".bin"
                },
                Sequence = (long)currentPartition,
                StartBlock = (long)currentBlock
            };
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
                        currentTapeFile = new TapeFileType
                        {
                            Image = new ImageType
                            {
                                format = "BINARY",
                                offset = (long)currentSize,
                                offsetSpecified = true,
                                Value = outputPrefix + ".bin"
                            },
                            Sequence = (long)currentFile,
                            StartBlock = (long)currentBlock,
                            BlockSize = blockSize
                        };
                        currentFileSize = 0;
                        fileChk = new Checksum();
                        files = new List<TapeFileType>();
                        currentTapePartition = new TapePartitionType
                        {
                            Image = new ImageType
                            {
                                format = "BINARY",
                                offset = (long)currentSize,
                                offsetSpecified = true,
                                Value = outputPrefix + ".bin"
                            },
                            Sequence = currentPartition,
                            StartBlock = (long)currentBlock
                        };
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

                sense = dev.Read6(out cmdBuf, out senseBuf, false, fixedLen, transferLen, blockSize, dev.Timeout, out duration);
                totalDuration += duration;

                if(sense)
                {
                    fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                    if(fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ == 0x00 && fxSense.Value.ILI && fxSense.Value.InformationValid)
                    {
                        blockSize = (uint)((int)blockSize - BitConverter.ToInt32(BitConverter.GetBytes(fxSense.Value.Information), 0));
                        currentTapeFile.BlockSize = blockSize;

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
                        if(fxSense.Value.ASC == 0x00 && (fxSense.Value.ASCQ == 0x02 || fxSense.Value.ASCQ == 0x05 || fxSense.Value.EOM))
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
                       (fxSense.Value.ASCQ == 0x02 || fxSense.Value.ASCQ == 0x05 || fxSense.Value.EOM))
                    {
                        // TODO: Detect end of partition
                        endOfMedia = true;
                        continue;
                    }

                    if((fxSense.Value.SenseKey == Decoders.SCSI.SenseKeys.NoSense || fxSense.Value.SenseKey == Decoders.SCSI.SenseKeys.RecoveredError) &&
                       (fxSense.Value.ASCQ == 0x01 || fxSense.Value.Filemark))
                    {
                        currentTapeFile.Checksums = fileChk.End().ToArray();
                        currentTapeFile.EndBlock = (long)(currentBlock - 1);
                        currentTapeFile.Size = (long)currentFileSize;
                        files.Add(currentTapeFile);

                        currentFile++;
                        currentTapeFile = new TapeFileType
                        {
                            Image = new ImageType
                            {
                                format = "BINARY",
                                offset = (long)currentSize,
                                offsetSpecified = true,
                                Value = outputPrefix + ".bin"
                            },
                            Sequence = (long)currentFile,
                            StartBlock = (long)currentBlock,
                            BlockSize = blockSize
                        };
                        currentFileSize = 0;
                        fileChk = new Checksum();

                        DicConsole.WriteLine();
                        DicConsole.WriteLine("Changed to file {0} at block {1}", currentFile, currentBlock);
                        continue;
                    }

                    // TODO: Add error recovering for tapes
                    fxSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf, out strSense);
                    DicConsole.ErrorWriteLine("Drive could not read block. Sense follows...");
                    DicConsole.ErrorWriteLine("{0} {1}", fxSense.Value.SenseKey, strSense);
                    return;
                }

                mhddLog.Write(currentBlock, duration);
                ibgLog.Write(currentBlock, currentSpeed * 1024);
                dumpFile.Write(cmdBuf);

                DateTime chkStart = DateTime.UtcNow;
                dataChk.Update(cmdBuf);
                fileChk.Update(cmdBuf);
                partitionChk.Update(cmdBuf);
                DateTime chkEnd = DateTime.UtcNow;
                double chkDuration = (chkEnd - chkStart).TotalMilliseconds;
                totalChkDuration += chkDuration;

                if(currentBlock % 10 == 0)
                    currentSpeed = ((double)blockSize / (double)1048576) / (duration / (double)1000);
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
            Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp, out string xmlDskSubTyp);
            sidecar.BlockMedia[0].DiskType = xmlDskTyp;
            sidecar.BlockMedia[0].DiskSubType = xmlDskSubTyp;
            // TODO: Implement device firmware revision
            sidecar.BlockMedia[0].Image = new ImageType
            {
                format = "Raw disk image (sector by sector copy)",
                Value = outputPrefix + ".bin"
            };
            sidecar.BlockMedia[0].LogicalBlocks = (long)blocks;
            sidecar.BlockMedia[0].Size = (long)(currentSize);
            sidecar.BlockMedia[0].DumpHardwareArray = new DumpHardwareType[1];
            sidecar.BlockMedia[0].DumpHardwareArray[0] = new DumpHardwareType
            {
                Extents = new ExtentType[1]
            };
            sidecar.BlockMedia[0].DumpHardwareArray[0].Extents[0] = new ExtentType
            {
                Start = 0,
                End = (int)(blocks - 1)
            };
            sidecar.BlockMedia[0].DumpHardwareArray[0].Manufacturer = dev.Manufacturer;
            sidecar.BlockMedia[0].DumpHardwareArray[0].Model = dev.Model;
            sidecar.BlockMedia[0].DumpHardwareArray[0].Revision = dev.Revision;
            sidecar.BlockMedia[0].DumpHardwareArray[0].Serial = dev.Serial;
            sidecar.BlockMedia[0].DumpHardwareArray[0].Software = Version.GetSoftwareType(dev.PlatformID);
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
    }
}
