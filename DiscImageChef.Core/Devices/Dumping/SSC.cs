// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SSC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps media from SCSI Streaming devices.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;
using Schemas;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using Version = DiscImageChef.CommonTypes.Metadata.Version;

namespace DiscImageChef.Core.Devices.Dumping
{
    // TODO: Add support for images
    partial class Dump
    {
        /// <summary>
        ///     Dumps the tape from a SCSI Streaming device
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="outputPrefix">Prefix for output data files</param>
        /// <param name="resume">Information for dump resuming</param>
        /// <param name="dumpLog">Dump logger</param>
        internal void Ssc(Device           dev, string outputPrefix, string devicePath, ref Resume resume,
                          ref DumpLog      dumpLog,
                          CICMMetadataType preSidecar)
        {
            FixedSense?      fxSense;
            bool             aborted;
            bool             sense;
            ulong            blocks = 0;
            uint             blockSize;
            MediaType        dskType = MediaType.Unknown;
            DateTime         start;
            DateTime         end;
            double           totalDuration    = 0;
            double           totalChkDuration = 0;
            double           currentSpeed     = 0;
            double           maxSpeed         = double.MinValue;
            double           minSpeed         = double.MaxValue;
            CICMMetadataType sidecar          = preSidecar ?? new CICMMetadataType();

            dev.RequestSense(out byte[] senseBuf, dev.Timeout, out double duration);
            fxSense = Sense.DecodeFixed(senseBuf, out string strSense);

            InitProgress?.Invoke();
            if(fxSense.HasValue && fxSense.Value.SenseKey != SenseKeys.NoSense)
            {
                dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h", fxSense.Value.SenseKey,
                                  fxSense.Value.ASC, fxSense.Value.ASCQ);
                StoppingErrorMessage?.Invoke("Drive has status error, please correct. Sense follows..." +
                                             Environment.NewLine                                        + strSense);
                return;
            }

            // Not in BOM/P
            if(fxSense.HasValue && fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ     != 0x00 &&
               fxSense.Value.ASCQ                    != 0x04 && fxSense.Value.SenseKey != SenseKeys.IllegalRequest)
            {
                dumpLog.WriteLine("Rewinding, please wait...");
                PulseProgress?.Invoke("Rewinding, please wait...");
                // Rewind, let timeout apply
                dev.Rewind(out senseBuf, dev.Timeout, out duration);

                // Still rewinding?
                // TODO: Pause?
                do
                {
                    PulseProgress?.Invoke("\rRewinding, please wait...");
                    dev.RequestSense(out senseBuf, dev.Timeout, out duration);
                    fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                }
                while(fxSense.HasValue && fxSense.Value.ASC == 0x00 &&
                      (fxSense.Value.ASCQ == 0x1A || fxSense.Value.ASCQ != 0x04));

                dev.RequestSense(out senseBuf, dev.Timeout, out duration);
                fxSense = Sense.DecodeFixed(senseBuf, out strSense);

                // And yet, did not rewind!
                if(fxSense.HasValue &&
                   (fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ != 0x04 || fxSense.Value.ASC != 0x00))
                {
                    StoppingErrorMessage?.Invoke("Drive could not rewind, please correct. Sense follows..." +
                                                 Environment.NewLine                                        + strSense);
                    dumpLog.WriteLine("Drive could not rewind, please correct. Sense follows...");
                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h", fxSense.Value.SenseKey,
                                      fxSense.Value.ASC, fxSense.Value.ASCQ);
                    return;
                }
            }

            // Check position
            sense = dev.ReadPosition(out byte[] cmdBuf, out senseBuf, SscPositionForms.Short, dev.Timeout,
                                     out duration);

            if(sense)
            {
                // READ POSITION is mandatory starting SCSI-2, so do not cry if the drive does not recognize the command (SCSI-1 or earlier)
                // Anyway, <=SCSI-1 tapes do not support partitions
                fxSense = Sense.DecodeFixed(senseBuf, out strSense);

                if(fxSense.HasValue && (fxSense.Value.ASC == 0x20 && fxSense.Value.ASCQ != 0x00 ||
                                        fxSense.Value.ASC      != 0x20 &&
                                        fxSense.Value.SenseKey != SenseKeys.IllegalRequest))
                {
                    StoppingErrorMessage?.Invoke("Could not get position. Sense follows..." + Environment.NewLine +
                                                 strSense);
                    dumpLog.WriteLine("Could not get position. Sense follows...");
                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h", fxSense.Value.SenseKey,
                                      fxSense.Value.ASC, fxSense.Value.ASCQ);
                    return;
                }
            }
            else
            {
                // Not in partition 0
                if(cmdBuf[1] != 0)
                {
                    UpdateStatus?.Invoke("Drive not in partition 0. Rewinding, please wait...");
                    dumpLog.WriteLine("Drive not in partition 0. Rewinding, please wait...");
                    // Rewind, let timeout apply
                    sense = dev.Locate(out senseBuf, false, 0, 0, dev.Timeout, out duration);
                    if(sense)
                    {
                        StoppingErrorMessage?.Invoke("Drive could not rewind, please correct. Sense follows..." +
                                                     Environment.NewLine                                        +
                                                     strSense);
                        dumpLog.WriteLine("Drive could not rewind, please correct. Sense follows...");
                        dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                          fxSense.Value.SenseKey, fxSense.Value.ASC, fxSense.Value.ASCQ);
                        return;
                    }

                    // Still rewinding?
                    // TODO: Pause?
                    do
                    {
                        Thread.Sleep(1000);
                        PulseProgress?.Invoke("\rRewinding, please wait...");
                        dev.RequestSense(out senseBuf, dev.Timeout, out duration);
                        fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                    }
                    while(fxSense.HasValue && fxSense.Value.ASC == 0x00 &&
                          (fxSense.Value.ASCQ == 0x1A || fxSense.Value.ASCQ == 0x19));

                    // And yet, did not rewind!
                    if(fxSense.HasValue && (fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ != 0x04 ||
                                            fxSense.Value.ASC != 0x00))
                    {
                        StoppingErrorMessage?.Invoke("Drive could not rewind, please correct. Sense follows..." +
                                                     Environment.NewLine                                        +
                                                     strSense);
                        dumpLog.WriteLine("Drive could not rewind, please correct. Sense follows...");
                        dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                          fxSense.Value.SenseKey, fxSense.Value.ASC, fxSense.Value.ASCQ);
                        return;
                    }

                    sense = dev.ReadPosition(out cmdBuf, out senseBuf, SscPositionForms.Short, dev.Timeout,
                                             out duration);
                    if(sense)
                    {
                        fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                        StoppingErrorMessage?.Invoke("Drive could not rewind, please correct. Sense follows..." +
                                                     Environment.NewLine                                        +
                                                     strSense);
                        dumpLog.WriteLine("Drive could not rewind, please correct. Sense follows...");
                        dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                          fxSense.Value.SenseKey, fxSense.Value.ASC, fxSense.Value.ASCQ);
                        return;
                    }

                    // Still not in partition 0!!!?
                    if(cmdBuf[1] != 0)
                    {
                        StoppingErrorMessage?.Invoke("Drive could not rewind to partition 0 but no error occurred...");
                        dumpLog.WriteLine("Drive could not rewind to partition 0 but no error occurred...");
                        return;
                    }
                }
            }

            EndProgress?.Invoke();

            sidecar.BlockMedia    = new BlockMediaType[1];
            sidecar.BlockMedia[0] = new BlockMediaType {SCSI = new SCSIType()};
            byte scsiMediumTypeTape  = 0;
            byte scsiDensityCodeTape = 0;

            UpdateStatus?.Invoke("Requesting MODE SENSE (10).");
            sense = dev.ModeSense10(out cmdBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F, 0xFF,
                                    5, out duration);
            if(!sense || dev.Error)
                sense = dev.ModeSense10(out cmdBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                        0x00, 5, out duration);

            Modes.DecodedMode? decMode = null;

            if(!sense && !dev.Error)
                if(Modes.DecodeMode10(cmdBuf, dev.ScsiType).HasValue)
                {
                    decMode = Modes.DecodeMode10(cmdBuf, dev.ScsiType);
                    sidecar.BlockMedia[0].SCSI.ModeSense10 = new DumpType
                    {
                        Image     = outputPrefix + ".modesense10.bin",
                        Size      = cmdBuf.Length,
                        Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                    };
                    DataFile.WriteTo("SCSI Dump", sidecar.BlockMedia[0].SCSI.ModeSense10.Image, cmdBuf);
                }

            UpdateStatus?.Invoke("Requesting MODE SENSE (6).");
            sense = dev.ModeSense6(out cmdBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5,
                                   out duration);
            if(sense || dev.Error)
                sense = dev.ModeSense6(out cmdBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5,
                                       out duration);
            if(sense || dev.Error) sense = dev.ModeSense(out cmdBuf, out senseBuf, 5, out duration);

            if(!sense && !dev.Error)
                if(Modes.DecodeMode6(cmdBuf, dev.ScsiType).HasValue)
                {
                    decMode = Modes.DecodeMode6(cmdBuf, dev.ScsiType);
                    sidecar.BlockMedia[0].SCSI.ModeSense = new DumpType
                    {
                        Image     = outputPrefix + ".modesense.bin",
                        Size      = cmdBuf.Length,
                        Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                    };
                    DataFile.WriteTo("SCSI Dump", sidecar.BlockMedia[0].SCSI.ModeSense.Image, cmdBuf);
                }

            // TODO: Check partitions page
            if(decMode.HasValue)
            {
                scsiMediumTypeTape = (byte)decMode.Value.Header.MediumType;
                if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length >= 1)
                    scsiDensityCodeTape = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                blockSize = decMode.Value.Header.BlockDescriptors[0].BlockLength;
                UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");
            }
            else blockSize = 1;

            if(dskType == MediaType.Unknown)
                dskType = MediaTypeFromScsi.Get((byte)dev.ScsiType, dev.Manufacturer, dev.Model, scsiMediumTypeTape,
                                                scsiDensityCodeTape, blocks, blockSize);

            UpdateStatus?.Invoke($"SCSI device type: {dev.ScsiType}.");
            UpdateStatus?.Invoke($"SCSI medium type: {scsiMediumTypeTape}.");
            UpdateStatus?.Invoke($"SCSI density type: {scsiDensityCodeTape}.");
            UpdateStatus?.Invoke($"Media identified as {dskType}.");

            dumpLog.WriteLine("SCSI device type: {0}.",   dev.ScsiType);
            dumpLog.WriteLine("SCSI medium type: {0}.",   scsiMediumTypeTape);
            dumpLog.WriteLine("SCSI density type: {0}.",  scsiDensityCodeTape);
            dumpLog.WriteLine("Media identified as {0}.", dskType);

            bool  endOfMedia           = false;
            ulong currentBlock         = 0;
            ulong currentFile          = 0;
            byte  currentPartition     = 0;
            byte  totalPartitions      = 1; // TODO: Handle partitions.
            ulong currentSize          = 0;
            ulong currentPartitionSize = 0;
            ulong currentFileSize      = 0;

            bool fixedLen    = false;
            uint transferLen = blockSize;

            sense = dev.Read6(out cmdBuf, out senseBuf, false, fixedLen, transferLen, blockSize, dev.Timeout,
                              out duration);
            if(sense)
            {
                fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                if(fxSense.HasValue)
                    if(fxSense.Value.SenseKey == SenseKeys.IllegalRequest)
                    {
                        sense = dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, dev.Timeout, out duration);
                        if(sense)
                        {
                            fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                            if(!fxSense.HasValue || !fxSense.Value.EOM)
                            {
                                StoppingErrorMessage?.Invoke("Drive could not return back. Sense follows..." +
                                                             Environment.NewLine                             +
                                                             strSense);
                                dumpLog.WriteLine("Drive could not return back. Sense follows...");
                                dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                  fxSense.Value.SenseKey, fxSense.Value.ASC, fxSense.Value.ASCQ);
                                return;
                            }
                        }

                        fixedLen    = true;
                        transferLen = 1;
                        sense = dev.Read6(out cmdBuf, out senseBuf, false, fixedLen, transferLen, blockSize,
                                          dev.Timeout, out duration);
                        if(sense)
                        {
                            StoppingErrorMessage?.Invoke("Drive could not read. Sense follows..." +
                                                         Environment.NewLine                      + strSense);
                            dumpLog.WriteLine("Drive could not read. Sense follows...");
                            dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                              fxSense.Value.SenseKey, fxSense.Value.ASC, fxSense.Value.ASCQ);
                            return;
                        }
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke("Drive could not read. Sense follows..." + Environment.NewLine +
                                                     strSense);
                        dumpLog.WriteLine("Drive could not read. Sense follows...");
                        dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                          fxSense.Value.SenseKey, fxSense.Value.ASC, fxSense.Value.ASCQ);
                        return;
                    }
                else
                {
                    StoppingErrorMessage?.Invoke("Cannot read device, don't know why, exiting...");
                    dumpLog.WriteLine("Cannot read device, don't know why, exiting...");
                    return;
                }
            }

            sense = dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, dev.Timeout, out duration);
            if(sense)
            {
                fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                if(!fxSense.HasValue || !fxSense.Value.EOM)
                {
                    StoppingErrorMessage?.Invoke("Drive could not return back. Sense follows..." + Environment.NewLine +
                                                 strSense);
                    dumpLog.WriteLine("Drive could not return back. Sense follows...");
                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h", fxSense.Value.SenseKey,
                                      fxSense.Value.ASC, fxSense.Value.ASCQ);
                    return;
                }
            }

            List<TapePartitionType> partitions = new List<TapePartitionType>();
            List<TapeFileType>      files      = new List<TapeFileType>();

            DataFile dumpFile = new DataFile(outputPrefix + ".bin");
            Checksum dataChk  = new Checksum();
            start = DateTime.UtcNow;
            MhddLog mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, 1);
            IbgLog  ibgLog  = new IbgLog(outputPrefix  + ".ibg", 0x0008);

            TapeFileType currentTapeFile = new TapeFileType
            {
                Image = new ImageType
                {
                    format          = "BINARY",
                    offset          = (long)currentSize,
                    offsetSpecified = true,
                    Value           = outputPrefix + ".bin"
                },
                Sequence   = (long)currentFile,
                StartBlock = (long)currentBlock,
                BlockSize  = blockSize
            };
            Checksum fileChk = new Checksum();
            TapePartitionType currentTapePartition = new TapePartitionType
            {
                Image = new ImageType
                {
                    format          = "BINARY",
                    offset          = (long)currentSize,
                    offsetSpecified = true,
                    Value           = outputPrefix + ".bin"
                },
                Sequence   = currentPartition,
                StartBlock = (long)currentBlock
            };
            Checksum partitionChk = new Checksum();

            aborted                       =  false;
            System.Console.CancelKeyPress += (sender, e) => e.Cancel = aborted = true;
            DateTime timeSpeedStart   = DateTime.UtcNow;
            ulong    currentSpeedSize = 0;

            InitProgress?.Invoke();
            while(currentPartition < totalPartitions)
            {
                if(aborted)
                {
                    UpdateStatus?.Invoke("Aborted!");
                    dumpLog.WriteLine("Aborted!");
                    break;
                }

                if(endOfMedia)
                {
                    UpdateStatus?.Invoke($"Finished partition {currentPartition}");
                    dumpLog.WriteLine("Finished partition {0}", currentPartition);
                    currentTapePartition.File      = files.ToArray();
                    currentTapePartition.Checksums = partitionChk.End().ToArray();
                    currentTapePartition.EndBlock  = (long)(currentBlock - 1);
                    currentTapePartition.Size      = (long)currentPartitionSize;
                    partitions.Add(currentTapePartition);

                    currentPartition++;

                    if(currentPartition < totalPartitions)
                    {
                        currentFile++;
                        currentTapeFile = new TapeFileType
                        {
                            Image = new ImageType
                            {
                                format          = "BINARY",
                                offset          = (long)currentSize,
                                offsetSpecified = true,
                                Value           = outputPrefix + ".bin"
                            },
                            Sequence   = (long)currentFile,
                            StartBlock = (long)currentBlock,
                            BlockSize  = blockSize
                        };
                        currentFileSize = 0;
                        fileChk         = new Checksum();
                        files           = new List<TapeFileType>();
                        currentTapePartition = new TapePartitionType
                        {
                            Image = new ImageType
                            {
                                format          = "BINARY",
                                offset          = (long)currentSize,
                                offsetSpecified = true,
                                Value           = outputPrefix + ".bin"
                            },
                            Sequence   = currentPartition,
                            StartBlock = (long)currentBlock
                        };
                        currentPartitionSize = 0;
                        partitionChk         = new Checksum();
                        UpdateStatus?.Invoke($"Seeking to partition {currentPartition}");
                        dev.Locate(out senseBuf, false, currentPartition, 0, dev.Timeout, out duration);
                        totalDuration += duration;
                    }

                    continue;
                }

                #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                PulseProgress?.Invoke($"\rReading block {currentBlock} ({currentSpeed:F3} MiB/sec.)");

                sense = dev.Read6(out cmdBuf, out senseBuf, false, fixedLen, transferLen, blockSize, dev.Timeout,
                                  out duration);
                totalDuration += duration;

                if(sense)
                {
                    fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                    if(fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ == 0x00 && fxSense.Value.ILI &&
                       fxSense.Value.InformationValid)
                    {
                        blockSize = (uint)((int)blockSize -
                                           BitConverter.ToInt32(BitConverter.GetBytes(fxSense.Value.Information), 0));
                        currentTapeFile.BlockSize = blockSize;

                        UpdateStatus?.Invoke($"Blocksize changed to {blockSize} bytes at block {currentBlock}");
                        dumpLog.WriteLine("Blocksize changed to {0} bytes at block {1}", blockSize, currentBlock);

                        sense = dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, dev.Timeout,
                                                   out duration);
                        totalDuration += duration;

                        if(sense)
                        {
                            fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                            StoppingErrorMessage?.Invoke("Drive could not go back one block. Sense follows..." +
                                                         Environment.NewLine                                   +
                                                         strSense);
                            dumpFile.Close();
                            dumpLog.WriteLine("Drive could not go back one block. Sense follows...");
                            dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                              fxSense.Value.SenseKey, fxSense.Value.ASC, fxSense.Value.ASCQ);
                            return;
                        }

                        continue;
                    }

                    switch(fxSense.Value.SenseKey)
                    {
                        case SenseKeys.BlankCheck when currentBlock == 0:
                            StoppingErrorMessage?.Invoke("Cannot dump a blank tape...");
                            dumpFile.Close();
                            dumpLog.WriteLine("Cannot dump a blank tape...");
                            return;
                        // For sure this is an end-of-tape/partition
                        case SenseKeys.BlankCheck when fxSense.Value.ASC == 0x00 &&
                                                       (fxSense.Value.ASCQ == 0x02 || fxSense.Value.ASCQ == 0x05 ||
                                                        fxSense.Value.EOM):
                            // TODO: Detect end of partition
                            endOfMedia = true;
                            UpdateStatus?.Invoke("Found end-of-tape/partition...");
                            dumpLog.WriteLine("Found end-of-tape/partition...");
                            continue;
                        case SenseKeys.BlankCheck:
                            StoppingErrorMessage?.Invoke("Blank block found, end of tape?...");
                            endOfMedia = true;
                            dumpLog.WriteLine("Blank block found, end of tape?...");
                            continue;
                    }

                    if((fxSense.Value.SenseKey == SenseKeys.NoSense ||
                        fxSense.Value.SenseKey == SenseKeys.RecoveredError) &&
                       (fxSense.Value.ASCQ == 0x02 || fxSense.Value.ASCQ == 0x05 || fxSense.Value.EOM))
                    {
                        // TODO: Detect end of partition
                        endOfMedia = true;
                        UpdateStatus?.Invoke("Found end-of-tape/partition...");
                        dumpLog.WriteLine("Found end-of-tape/partition...");
                        continue;
                    }

                    if((fxSense.Value.SenseKey == SenseKeys.NoSense ||
                        fxSense.Value.SenseKey == SenseKeys.RecoveredError) &&
                       (fxSense.Value.ASCQ == 0x01 || fxSense.Value.Filemark))
                    {
                        currentTapeFile.Checksums = fileChk.End().ToArray();
                        currentTapeFile.EndBlock  = (long)(currentBlock - 1);
                        currentTapeFile.Size      = (long)currentFileSize;
                        files.Add(currentTapeFile);

                        currentFile++;
                        currentTapeFile = new TapeFileType
                        {
                            Image = new ImageType
                            {
                                format          = "BINARY",
                                offset          = (long)currentSize,
                                offsetSpecified = true,
                                Value           = outputPrefix + ".bin"
                            },
                            Sequence   = (long)currentFile,
                            StartBlock = (long)currentBlock,
                            BlockSize  = blockSize
                        };
                        currentFileSize = 0;
                        fileChk         = new Checksum();

                        UpdateStatus?.Invoke($"Changed to file {currentFile} at block {currentBlock}");
                        dumpLog.WriteLine("Changed to file {0} at block {1}", currentFile, currentBlock);
                        continue;
                    }

                    // TODO: Add error recovering for tapes
                    fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                    StoppingErrorMessage
                      ?.Invoke($"Drive could not read block. Sense follows...\n{fxSense.Value.SenseKey} {strSense}");
                    dumpLog.WriteLine("Drive could not read block. Sense follows...");
                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h", fxSense.Value.SenseKey,
                                      fxSense.Value.ASC, fxSense.Value.ASCQ);
                    return;
                }

                mhddLog.Write(currentBlock, duration);
                ibgLog.Write(currentBlock, currentSpeed * 1024);
                dumpFile.Write(cmdBuf);

                DateTime chkStart = DateTime.UtcNow;
                dataChk.Update(cmdBuf);
                fileChk.Update(cmdBuf);
                partitionChk.Update(cmdBuf);
                DateTime chkEnd      = DateTime.UtcNow;
                double   chkDuration = (chkEnd - chkStart).TotalMilliseconds;
                totalChkDuration += chkDuration;

                currentBlock++;
                currentSize          += blockSize;
                currentFileSize      += blockSize;
                currentPartitionSize += blockSize;
                currentSpeedSize     += blockSize;

                double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;
                if(elapsed < 1) continue;

                currentSpeed     = currentSpeedSize / (1048576 * elapsed);
                currentSpeedSize = 0;
                timeSpeedStart   = DateTime.UtcNow;
            }

            EndProgress?.Invoke();

            blocks = currentBlock + 1;
            end    = DateTime.UtcNow;
            mhddLog.Close();
            ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                         blockSize * (double)(blocks + 1) / 1024                          / (totalDuration / 1000),
                         devicePath);
            dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));
            dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

            UpdateStatus
              ?.Invoke($"Took a total of {(end - start).TotalSeconds:F3} seconds ({totalDuration / 1000:F3} processing commands, {totalChkDuration / 1000:F3} checksumming).");
            UpdateStatus
              ?.Invoke($"Average speed: {(double)blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000):F3} MiB/sec.");
            UpdateStatus?.Invoke($"Fastest speed burst: {maxSpeed:F3} MiB/sec.");
            UpdateStatus?.Invoke($"Slowest speed burst: {minSpeed:F3} MiB/sec.");

            sidecar.BlockMedia[0].Checksums  = dataChk.End().ToArray();
            sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);
            CommonTypes.Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp, out string xmlDskSubTyp);
            sidecar.BlockMedia[0].DiskType    = xmlDskTyp;
            sidecar.BlockMedia[0].DiskSubType = xmlDskSubTyp;
            // TODO: Implement device firmware revision
            sidecar.BlockMedia[0].Image = new ImageType
            {
                format = "Raw disk image (sector by sector copy)", Value = outputPrefix + ".bin"
            };
            sidecar.BlockMedia[0].LogicalBlocks     = (long)blocks;
            sidecar.BlockMedia[0].Size              = (long)currentSize;
            sidecar.BlockMedia[0].DumpHardwareArray = new DumpHardwareType[1];
            sidecar.BlockMedia[0].DumpHardwareArray[0] =
                new DumpHardwareType {Extents = new ExtentType[1]};
            sidecar.BlockMedia[0].DumpHardwareArray[0].Extents[0] =
                new ExtentType {Start = 0, End = blocks - 1};
            sidecar.BlockMedia[0].DumpHardwareArray[0].Manufacturer = dev.Manufacturer;
            sidecar.BlockMedia[0].DumpHardwareArray[0].Model        = dev.Model;
            sidecar.BlockMedia[0].DumpHardwareArray[0].Revision     = dev.Revision;
            sidecar.BlockMedia[0].DumpHardwareArray[0].Serial       = dev.Serial;
            sidecar.BlockMedia[0].DumpHardwareArray[0].Software     = Version.GetSoftwareType();
            sidecar.BlockMedia[0].TapeInformation                   = partitions.ToArray();

            if(!aborted)
            {
                UpdateStatus?.Invoke("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml", FileMode.Create);

                XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }

            Statistics.AddMedia(dskType, true);
        }
    }
}