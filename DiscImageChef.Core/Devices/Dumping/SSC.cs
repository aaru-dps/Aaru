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
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Extents;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;
using Schemas;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using Version = DiscImageChef.CommonTypes.Metadata.Version;

namespace DiscImageChef.Core.Devices.Dumping
{
    partial class Dump
    {
        /// <summary>
        ///     Dumps the tape from a SCSI Streaming device
        /// </summary>
        internal void Ssc()
        {
            FixedSense?      fxSense;
            bool             sense;
            uint             blockSize;
            ulong            blocks  = 0;
            MediaType        dskType = MediaType.Unknown;
            DateTime         start;
            DateTime         end;
            double           totalDuration    = 0;
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
                    PulseProgress?.Invoke("Rewinding, please wait...");
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
                        PulseProgress?.Invoke("Rewinding, please wait...");
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
                        Size      = (ulong)cmdBuf.Length,
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
                        Size      = (ulong)cmdBuf.Length,
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
                blockSize = decMode.Value.Header.BlockDescriptors?[0].BlockLength ?? 0;

                UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");
            }
            else blockSize = 1;

            if(blockSize == 0) blockSize = 1;

            if(dskType == MediaType.Unknown)
                dskType = MediaTypeFromScsi.Get((byte)dev.ScsiType, dev.Manufacturer, dev.Model, scsiMediumTypeTape,
                                                scsiDensityCodeTape, blocks, blockSize);
            if(dskType == MediaType.Unknown) dskType = MediaType.UnknownTape;

            UpdateStatus?.Invoke($"SCSI device type: {dev.ScsiType}.");
            UpdateStatus?.Invoke($"SCSI medium type: {scsiMediumTypeTape}.");
            UpdateStatus?.Invoke($"SCSI density type: {scsiDensityCodeTape}.");
            UpdateStatus?.Invoke($"Media identified as {dskType}.");

            dumpLog.WriteLine("SCSI device type: {0}.",   dev.ScsiType);
            dumpLog.WriteLine("SCSI medium type: {0}.",   scsiMediumTypeTape);
            dumpLog.WriteLine("SCSI density type: {0}.",  scsiDensityCodeTape);
            dumpLog.WriteLine("Media identified as {0}.", dskType);

            bool  endOfMedia       = false;
            ulong currentBlock     = 0;
            uint  currentFile      = 0;
            byte  currentPartition = 0;
            byte  totalPartitions  = 1; // TODO: Handle partitions.
            bool  fixedLen         = false;
            uint  transferLen      = blockSize;

            firstRead:
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
                                dumpLog.WriteLine("Device not ready. Sense {0} ASC {1:X2}h ASCQ {2:X2}h",
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
                            fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                            StoppingErrorMessage?.Invoke("Drive could not read. Sense follows..." +
                                                         Environment.NewLine                      + strSense);
                            dumpLog.WriteLine("Drive could not read. Sense follows...");
                            dumpLog.WriteLine("Device not ready. Sense {0} ASC {1:X2}h ASCQ {2:X2}h",
                                              fxSense.Value.SenseKey, fxSense.Value.ASC, fxSense.Value.ASCQ);
                            return;
                        }
                    }
                    else if(fxSense.Value.ASC == 0x00 && fxSense.Value.ASCQ == 0x00 && fxSense.Value.ILI &&
                            fxSense.Value.InformationValid)
                    {
                        blockSize = (uint)((int)blockSize -
                                           BitConverter.ToInt32(BitConverter.GetBytes(fxSense.Value.Information), 0));
                        transferLen = blockSize;

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
                            dumpLog.WriteLine("Drive could not go back one block. Sense follows...");
                            dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                              fxSense.Value.SenseKey, fxSense.Value.ASC, fxSense.Value.ASCQ);
                            return;
                        }

                        goto firstRead;
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke("Drive could not read. Sense follows..." + Environment.NewLine +
                                                     strSense);
                        dumpLog.WriteLine("Drive could not read. Sense follows...");
                        dumpLog.WriteLine("Device not ready. Sense {0} ASC {1:X2}h ASCQ {2:X2}h",
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
                    dumpLog.WriteLine("Device not ready. Sense {0} ASC {1:X2}h ASCQ {2:X2}h", fxSense.Value.SenseKey,
                                      fxSense.Value.ASC, fxSense.Value.ASCQ);
                    return;
                }
            }

            DumpHardwareType currentTry = null;
            ExtentsULong     extents    = null;
            ResumeSupport.Process(true, dev.IsRemovable, blocks, dev.Manufacturer, dev.Model, dev.Serial,
                                  dev.PlatformId, ref resume, ref currentTry, ref extents, true);

            bool rewind = false;
            if(resume.NextBlock > 0)
            {
                UpdateStatus?.Invoke($"Positioning tape to block {resume.NextBlock}.");
                dumpLog.WriteLine("Positioning tape to block {0}.", resume.NextBlock);
                if(resume.NextBlock > uint.MaxValue)
                {
                    sense = dev.Locate16(out senseBuf, resume.NextBlock, dev.Timeout, out _);

                    if(!sense)
                    {
                        sense = dev.ReadPositionLong(out cmdBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            if(!force)
                            {
                                dumpLog.WriteLine("Could not check current position, unable to resume. If you want to continue use force.");
                                StoppingErrorMessage?.Invoke("Could not check current position, unable to resume. If you want to continue use force.");
                            }
                            else
                            {
                                dumpLog.WriteLine("Could not check current position, unable to resume. Dumping from the start.");
                                ErrorMessage?.Invoke("Could not check current position, unable to resume. Dumping from the start.");
                                rewind = true;
                            }
                        }
                        else
                        {
                            ulong position = Swapping.Swap(BitConverter.ToUInt64(cmdBuf, 8));

                            if(position != resume.NextBlock)
                            {
                                if(!force)
                                {
                                    dumpLog.WriteLine("Current position is not as expected, unable to resume. If you want to continue use force.");
                                    StoppingErrorMessage?.Invoke("Current position is not as expected, unable to resume. If you want to continue use force.");
                                }
                                else
                                {
                                    dumpLog.WriteLine("Current position is not as expected, unable to resume. Dumping from the start.");
                                    ErrorMessage?.Invoke("Current position is not as expected, unable to resume. Dumping from the start.");
                                    rewind = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if(!force)
                        {
                            dumpLog.WriteLine("Cannot reposition tape, unable to resume. If you want to continue use force.");
                            StoppingErrorMessage?.Invoke("Cannot reposition tape, unable to resume. If you want to continue use force.");
                        }
                        else
                        {
                            dumpLog.WriteLine("Cannot reposition tape, unable to resume. Dumping from the start.");
                            ErrorMessage?.Invoke("Cannot reposition tape, unable to resume. Dumping from the start.");
                            rewind = true;
                        }
                    }
                }
                else
                {
                    sense = dev.Locate(out senseBuf, (uint)resume.NextBlock, dev.Timeout, out _);

                    if(!sense)
                    {
                        sense = dev.ReadPosition(out cmdBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            if(!force)
                            {
                                dumpLog.WriteLine("Could not check current position, unable to resume. If you want to continue use force.");
                                StoppingErrorMessage?.Invoke("Could not check current position, unable to resume. If you want to continue use force.");
                            }
                            else
                            {
                                dumpLog.WriteLine("Could not check current position, unable to resume. Dumping from the start.");
                                ErrorMessage?.Invoke("Could not check current position, unable to resume. Dumping from the start.");
                                rewind = true;
                            }
                        }
                        else
                        {
                            ulong position = Swapping.Swap(BitConverter.ToUInt32(cmdBuf, 4));

                            if(position != resume.NextBlock)
                            {
                                if(!force)
                                {
                                    dumpLog.WriteLine("Current position is not as expected, unable to resume. If you want to continue use force.");
                                    StoppingErrorMessage?.Invoke("Current position is not as expected, unable to resume. If you want to continue use force.");
                                }
                                else
                                {
                                    dumpLog.WriteLine("Current position is not as expected, unable to resume. Dumping from the start.");
                                    ErrorMessage?.Invoke("Current position is not as expected, unable to resume. Dumping from the start.");
                                    rewind = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if(!force)
                        {
                            dumpLog.WriteLine("Cannot reposition tape, unable to resume. If you want to continue use force.");
                            StoppingErrorMessage?.Invoke("Cannot reposition tape, unable to resume. If you want to continue use force.");
                        }
                        else
                        {
                            dumpLog.WriteLine("Cannot reposition tape, unable to resume. Dumping from the start.");
                            ErrorMessage?.Invoke("Cannot reposition tape, unable to resume. Dumping from the start.");
                            rewind = true;
                        }
                    }
                }
            }

            if(rewind)
            {
                do
                {
                    Thread.Sleep(1000);
                    PulseProgress?.Invoke("Rewinding, please wait...");
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
            }

            bool ret = (outputPlugin as IWritableTapeImage).SetTape();
            // Cannot set image to tape mode
            if(!ret)
            {
                dumpLog.WriteLine("Error setting output image in tape mode, not continuing.");
                dumpLog.WriteLine(outputPlugin.ErrorMessage);
                StoppingErrorMessage?.Invoke("Error setting output image in tape mode, not continuing." + Environment.NewLine +
                                             outputPlugin.ErrorMessage);
                return;
            }

            ret = outputPlugin.Create(outputPath, dskType, formatOptions, 0, 0);

            // Cannot create image
            if(!ret)
            {
                dumpLog.WriteLine("Error creating output image, not continuing.");
                dumpLog.WriteLine(outputPlugin.ErrorMessage);
                StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                             outputPlugin.ErrorMessage);
                return;
            }

            start = DateTime.UtcNow;
            MhddLog mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, 1);
            IbgLog  ibgLog  = new IbgLog(outputPrefix  + ".ibg", 0x0008);

            TapeFile currentTapeFile =
                new TapeFile {File = currentFile, FirstBlock = currentBlock, Partition = currentPartition};
            TapePartition currentTapePartition =
                new TapePartition {Number = currentPartition, FirstBlock = currentBlock};

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

                    currentTapeFile.LastBlock = currentBlock - 1;
                    (outputPlugin as IWritableTapeImage).AddFile(currentTapeFile);

                    currentTapePartition.LastBlock = currentBlock - 1;
                    (outputPlugin as IWritableTapeImage).AddPartition(currentTapePartition);

                    currentPartition++;

                    if(currentPartition < totalPartitions)
                    {
                        currentFile++;
                        currentTapeFile = new TapeFile
                        {
                            File = currentFile, FirstBlock = currentBlock, Partition = currentPartition
                        };
                        currentTapePartition = new TapePartition {Number = currentPartition, FirstBlock = currentBlock};
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

                PulseProgress?.Invoke($"Reading block {currentBlock} ({currentSpeed:F3} MiB/sec.)");

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
                        if(!fixedLen) transferLen = blockSize;

                        UpdateStatus?.Invoke($"Blocksize changed to {blockSize} bytes at block {currentBlock}");
                        dumpLog.WriteLine("Blocksize changed to {0} bytes at block {1}", blockSize, currentBlock);

                        sense = dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, dev.Timeout, out duration);
                        totalDuration += duration;

                        if(sense)
                        {
                            fxSense = Sense.DecodeFixed(senseBuf, out strSense);
                            StoppingErrorMessage?.Invoke("Drive could not go back one block. Sense follows..." +
                                                         Environment.NewLine                                   +
                                                         strSense);
                            outputPlugin.Close();
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
                            outputPlugin.Close();
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
                        currentTapeFile.LastBlock = currentBlock - 1;
                        (outputPlugin as IWritableTapeImage).AddFile(currentTapeFile);

                        currentFile++;
                        currentTapeFile = new TapeFile
                        {
                            File = currentFile, FirstBlock = currentBlock, Partition = currentPartition
                        };

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
                outputPlugin.WriteSector(cmdBuf, currentBlock);

                currentBlock++;
                resume.NextBlock++;
                currentSpeedSize += blockSize;

                double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;
                if(elapsed < 1) continue;

                currentSpeed     = currentSpeedSize / (1048576 * elapsed);
                currentSpeedSize = 0;
                timeSpeedStart   = DateTime.UtcNow;
            }

            EndProgress?.Invoke();

            currentTapeFile.LastBlock = currentBlock - 1;
            (outputPlugin as IWritableTapeImage).AddFile(currentTapeFile);
            currentTapePartition.LastBlock = currentBlock - 1;
            (outputPlugin as IWritableTapeImage).AddPartition(currentTapePartition);

            outputPlugin.SetDumpHardware(resume.Tries);
            if(preSidecar != null) outputPlugin.SetCicmMetadata(preSidecar);
            dumpLog.WriteLine("Closing output file.");
            UpdateStatus?.Invoke("Closing output file.");
            DateTime closeStart = DateTime.Now;
            outputPlugin.Close();
            DateTime closeEnd = DateTime.Now;
            UpdateStatus?.Invoke($"Closed in {(closeEnd - closeStart).TotalSeconds} seconds.");
            dumpLog.WriteLine("Closed in {0} seconds.", (closeEnd - closeStart).TotalSeconds);

            blocks = currentBlock + 1;
            end    = DateTime.UtcNow;
            mhddLog.Close();
            ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                         blockSize * (double)(blocks + 1) / 1024                          / (totalDuration / 1000),
                         devicePath);
            dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));

            UpdateStatus
              ?.Invoke($"Average speed: {(double)blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000):F3} MiB/sec.");
            UpdateStatus?.Invoke($"Fastest speed burst: {maxSpeed:F3} MiB/sec.");
            UpdateStatus?.Invoke($"Slowest speed burst: {minSpeed:F3} MiB/sec.");

            // TODO: Media sidecar

            Statistics.AddMedia(dskType, true);
        }
    }
}