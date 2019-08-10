// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SBC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps SCSI Block devices.
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
using System.Linq;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Extents;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;
using Schemas;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using TrackType = DiscImageChef.CommonTypes.Enums.TrackType;

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>
    ///     Implements dumping SCSI Block Commands and Reduced Block Commands devices
    /// </summary>
    partial class Dump
    {
        /// <summary>
        ///     Dumps a SCSI Block Commands device or a Reduced Block Commands devices
        /// </summary>
        /// <param name="opticalDisc">If device contains an optical disc (e.g. DVD or BD)</param>
        /// <param name="mediaTags">Media tags as retrieved in MMC layer</param>
        /// <param name="dskType">Disc type as detected in SCSI or MMC layer</param>
        internal void Sbc(Dictionary<MediaTagType, byte[]> mediaTags, ref MediaType dskType, bool opticalDisc)
        {
            bool               sense;
            byte               scsiMediumType     = 0;
            byte               scsiDensityCode    = 0;
            bool               containsFloppyPage = false;
            const ushort       SBC_PROFILE        = 0x0001;
            DateTime           start;
            DateTime           end;
            double             totalDuration = 0;
            double             currentSpeed  = 0;
            double             maxSpeed      = double.MinValue;
            double             minSpeed      = double.MaxValue;
            byte[]             readBuffer;
            Modes.DecodedMode? decMode = null;

            if(opticalDisc)
                switch(dskType)
                {
                    case MediaType.REV35:
                    case MediaType.REV70:
                    case MediaType.REV120:
                        opticalDisc = false;
                        break;
                }

            dumpLog.WriteLine("Initializing reader.");
            Reader scsiReader = new Reader(dev, dev.Timeout, null, dumpRaw);
            ulong  blocks     = scsiReader.GetDeviceBlocks();
            uint   blockSize  = scsiReader.LogicalBlockSize;
            if(scsiReader.FindReadCommand())
            {
                dumpLog.WriteLine("ERROR: Cannot find correct read command: {0}.", scsiReader.ErrorMessage);
                StoppingErrorMessage?.Invoke("Unable to read medium.");
                return;
            }

            if(blocks != 0 && blockSize != 0)
            {
                blocks++;
                UpdateStatus
                  ?.Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {blocks * (ulong)blockSize} bytes)");
            }

            // Check how many blocks to read, if error show and return
            if(scsiReader.GetBlocksToRead())
            {
                dumpLog.WriteLine("ERROR: Cannot get blocks to read: {0}.", scsiReader.ErrorMessage);
                StoppingErrorMessage?.Invoke(scsiReader.ErrorMessage);
                return;
            }

            uint blocksToRead      = scsiReader.BlocksToRead;
            uint logicalBlockSize  = blockSize;
            uint physicalBlockSize = scsiReader.PhysicalBlockSize;

            if(blocks == 0)
            {
                dumpLog.WriteLine("ERROR: Unable to read medium or empty medium present...");
                StoppingErrorMessage?.Invoke("Unable to read medium or empty medium present...");
                return;
            }

            if(!opticalDisc)
            {
                mediaTags = new Dictionary<MediaTagType, byte[]>();

                if(dev.IsUsb && dev.UsbDescriptors != null) mediaTags.Add(MediaTagType.USB_Descriptors, null);
                if(dev.Type == DeviceType.ATAPI) mediaTags.Add(MediaTagType.ATAPI_IDENTIFY,             null);
                if(dev.IsPcmcia && dev.Cis != null) mediaTags.Add(MediaTagType.PCMCIA_CIS,              null);

                sense = dev.ScsiInquiry(out byte[] cmdBuf, out _);
                mediaTags.Add(MediaTagType.SCSI_INQUIRY, cmdBuf);
                if(!sense)
                {
                    dumpLog.WriteLine("Requesting MODE SENSE (10).");
                    UpdateStatus?.Invoke("Requesting MODE SENSE (10).");
                    sense = dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                            0xFF, 5, out _);
                    if(!sense || dev.Error)
                        sense = dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                                0x00, 5, out _);

                    if(!sense && !dev.Error)
                        if(Modes.DecodeMode10(cmdBuf, dev.ScsiType).HasValue)
                        {
                            mediaTags.Add(MediaTagType.SCSI_MODESENSE_10, cmdBuf);
                            decMode = Modes.DecodeMode10(cmdBuf, dev.ScsiType);
                        }

                    dumpLog.WriteLine("Requesting MODE SENSE (6).");
                    UpdateStatus?.Invoke("Requesting MODE SENSE (6).");
                    sense = dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5,
                                           out _);
                    if(sense || dev.Error)
                        sense = dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F, 0x00,
                                               5, out _);
                    if(sense || dev.Error) sense = dev.ModeSense(out cmdBuf, out _, 5, out _);

                    if(!sense && !dev.Error)
                        if(Modes.DecodeMode6(cmdBuf, dev.ScsiType).HasValue)
                        {
                            mediaTags.Add(MediaTagType.SCSI_MODESENSE_6, cmdBuf);
                            decMode = Modes.DecodeMode6(cmdBuf, dev.ScsiType);
                        }

                    if(decMode.HasValue)
                    {
                        scsiMediumType = (byte)decMode.Value.Header.MediumType;
                        if(decMode.Value.Header.BlockDescriptors        != null &&
                           decMode.Value.Header.BlockDescriptors.Length >= 1)
                            scsiDensityCode = (byte)decMode.Value.Header.BlockDescriptors[0].Density;

                        containsFloppyPage =
                            decMode.Value.Pages.Aggregate(containsFloppyPage,
                                                          (current, modePage) => current | (modePage.Page == 0x05));
                    }
                }
            }

            if(dskType == MediaType.Unknown)
                dskType = MediaTypeFromScsi.Get((byte)dev.ScsiType, dev.Manufacturer, dev.Model, scsiMediumType,
                                                scsiDensityCode, blocks, blockSize);

            if(dskType == MediaType.Unknown && dev.IsUsb && containsFloppyPage) dskType = MediaType.FlashDrive;

            UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");
            UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time.");
            UpdateStatus?.Invoke($"Device reports {blockSize} bytes per logical block.");
            UpdateStatus?.Invoke($"Device reports {scsiReader.LongBlockSize} bytes per physical block.");
            UpdateStatus?.Invoke($"SCSI device type: {dev.ScsiType}.");
            UpdateStatus?.Invoke($"SCSI medium type: {scsiMediumType}.");
            UpdateStatus?.Invoke($"SCSI density type: {scsiDensityCode}.");
            UpdateStatus?.Invoke($"SCSI floppy mode page present: {containsFloppyPage}.");
            UpdateStatus?.Invoke($"Media identified as {dskType}");

            dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).",       blocks, blocks * blockSize);
            dumpLog.WriteLine("Device can read {0} blocks at a time.",        blocksToRead);
            dumpLog.WriteLine("Device reports {0} bytes per logical block.",  blockSize);
            dumpLog.WriteLine("Device reports {0} bytes per physical block.", scsiReader.LongBlockSize);
            dumpLog.WriteLine("SCSI device type: {0}.",                       dev.ScsiType);
            dumpLog.WriteLine("SCSI medium type: {0}.",                       scsiMediumType);
            dumpLog.WriteLine("SCSI density type: {0}.",                      scsiDensityCode);
            dumpLog.WriteLine("SCSI floppy mode page present: {0}.",          containsFloppyPage);
            dumpLog.WriteLine("Media identified as {0}.",                     dskType);

            uint longBlockSize = scsiReader.LongBlockSize;

            if(dumpRaw)
                if(blockSize == longBlockSize)
                {
                    ErrorMessage?.Invoke(!scsiReader.CanReadRaw
                                             ? "Device doesn't seem capable of reading raw data from media."
                                             : "Device is capable of reading raw data but I've been unable to guess correct sector size.");

                    if(!force)
                    {
                        StoppingErrorMessage
                          ?.Invoke("Not continuing. If you want to continue reading cooked data when raw is not available use the force option.");
                        // TODO: Exit more gracefully
                        return;
                    }

                    ErrorMessage?.Invoke("Continuing dumping cooked data.");
                }
                else
                {
                    // Only a block will be read, but it contains 16 sectors and command expect sector number not block number
                    blocksToRead = (uint)(longBlockSize == 37856 ? 16 : 1);
                    UpdateStatus
                      ?.Invoke($"Reading {longBlockSize} raw bytes ({blockSize * blocksToRead} cooked bytes) per sector.");
                    physicalBlockSize = longBlockSize;
                    blockSize         = longBlockSize;
                }

            bool ret = true;

            foreach(MediaTagType tag in mediaTags.Keys)
            {
                if(outputPlugin.SupportedMediaTags.Contains(tag)) continue;

                ret = false;
                dumpLog.WriteLine($"Output format does not support {tag}.");
                ErrorMessage?.Invoke($"Output format does not support {tag}.");
            }

            if(!ret)
            {
                if(force)
                {
                    dumpLog.WriteLine("Several media tags not supported, continuing...");
                    ErrorMessage?.Invoke("Several media tags not supported, continuing...");
                }
                else
                {
                    dumpLog.WriteLine("Several media tags not supported, not continuing...");
                    StoppingErrorMessage?.Invoke("Several media tags not supported, not continuing...");
                    return;
                }
            }

            UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time.");
            dumpLog.WriteLine("Reading {0} sectors at a time.", blocksToRead);

            MhddLog mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
            IbgLog  ibgLog  = new IbgLog(outputPrefix  + ".ibg", SBC_PROFILE);
            ret = outputPlugin.Create(outputPath, dskType, formatOptions, blocks, blockSize);

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
            double imageWriteDuration = 0;

            if(opticalDisc)
            {
                if(outputPlugin is IWritableOpticalImage opticalPlugin)
                    opticalPlugin.SetTracks(new List<Track>
                    {
                        new Track
                        {
                            TrackBytesPerSector    = (int)blockSize,
                            TrackEndSector         = blocks - 1,
                            TrackSequence          = 1,
                            TrackRawBytesPerSector = (int)blockSize,
                            TrackSubchannelType    = TrackSubchannelType.None,
                            TrackSession           = 1,
                            TrackType              = TrackType.Data
                        }
                    });
                else
                {
                    dumpLog.WriteLine("The specified plugin does not support storing optical disc images..");
                    StoppingErrorMessage?.Invoke("The specified plugin does not support storing optical disc images.");
                    return;
                }
            }
            else if(decMode.HasValue)
            {
                bool setGeometry = false;

                foreach(Modes.ModePage page in decMode.Value.Pages)
                    if(page.Page == 0x04 && page.Subpage == 0x00)
                    {
                        Modes.ModePage_04? rigidPage = Modes.DecodeModePage_04(page.PageResponse);
                        if(!rigidPage.HasValue || setGeometry) continue;

                        dumpLog.WriteLine("Setting geometry to {0} cylinders, {1} heads, {2} sectors per track",
                                          rigidPage.Value.Cylinders, rigidPage.Value.Heads,
                                          (uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads)));
                        UpdateStatus
                          ?.Invoke($"Setting geometry to {rigidPage.Value.Cylinders} cylinders, {rigidPage.Value.Heads} heads, {(uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads))} sectors per track");
                        outputPlugin.SetGeometry(rigidPage.Value.Cylinders, rigidPage.Value.Heads,
                                                 (uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads)));

                        setGeometry = true;
                    }
                    else if(page.Page == 0x05 && page.Subpage == 0x00)
                    {
                        Modes.ModePage_05? flexiblePage = Modes.DecodeModePage_05(page.PageResponse);
                        if(!flexiblePage.HasValue) continue;

                        dumpLog.WriteLine("Setting geometry to {0} cylinders, {1} heads, {2} sectors per track",
                                          flexiblePage.Value.Cylinders, flexiblePage.Value.Heads,
                                          flexiblePage.Value.SectorsPerTrack);
                        UpdateStatus
                          ?.Invoke($"Setting geometry to {flexiblePage.Value.Cylinders} cylinders, {flexiblePage.Value.Heads} heads, {flexiblePage.Value.SectorsPerTrack} sectors per track");
                        outputPlugin.SetGeometry(flexiblePage.Value.Cylinders, flexiblePage.Value.Heads,
                                                 flexiblePage.Value.SectorsPerTrack);
                        setGeometry = true;
                    }
            }

            DumpHardwareType currentTry = null;
            ExtentsULong     extents    = null;
            ResumeSupport.Process(true, dev.IsRemovable, blocks, dev.Manufacturer, dev.Model, dev.Serial,
                                  dev.PlatformId, ref resume, ref currentTry, ref extents);
            if(currentTry == null || extents == null)
            {
                StoppingErrorMessage?.Invoke("Could not process resume file, not continuing...");
                return;
            }

            if(resume.NextBlock > 0)
            {
                UpdateStatus?.Invoke($"Resuming from block {resume.NextBlock}.");
                dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);
            }

            bool     newTrim          = false;
            DateTime timeSpeedStart   = DateTime.UtcNow;
            ulong    sectorSpeedStart = 0;
            InitProgress?.Invoke();
            for(ulong i = resume.NextBlock; i < blocks; i += blocksToRead)
            {
                if(aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke("Aborted!");
                    dumpLog.WriteLine("Aborted!");
                    break;
                }

                if(blocks - i < blocksToRead) blocksToRead = (uint)(blocks - i);

                #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                UpdateProgress?.Invoke($"Reading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)", (long)i,
                                       (long)blocks);

                sense         =  scsiReader.ReadBlocks(out readBuffer, i, blocksToRead, out double cmdDuration);
                totalDuration += cmdDuration;

                if(!sense && !dev.Error)
                {
                    mhddLog.Write(i, cmdDuration);
                    ibgLog.Write(i, currentSpeed * 1024);
                    DateTime writeStart = DateTime.Now;
                    outputPlugin.WriteSectors(readBuffer, i, blocksToRead);
                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                    extents.Add(i, blocksToRead, true);
                }
                else
                {
                    // TODO: Reset device after X errors
                    if(stopOnError) return; // TODO: Return more cleanly

                    if(i + skip > blocks) skip = (uint)(blocks - i);

                    // Write empty data
                    DateTime writeStart = DateTime.Now;
                    outputPlugin.WriteSectors(new byte[blockSize * skip], i, skip);
                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                    for(ulong b = i; b < i + skip; b++) resume.BadBlocks.Add(b);

                    mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                    ibgLog.Write(i, 0);
                    dumpLog.WriteLine("Skipping {0} blocks from errored block {1}.", skip, i);
                    i       += skip - blocksToRead;
                    newTrim =  true;
                }

                sectorSpeedStart += blocksToRead;
                resume.NextBlock =  i + blocksToRead;

                double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;
                if(elapsed < 1) continue;

                currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                sectorSpeedStart = 0;
                timeSpeedStart   = DateTime.UtcNow;
            }

            end = DateTime.UtcNow;
            EndProgress?.Invoke();
            mhddLog.Close();
            ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                         blockSize * (double)(blocks + 1) / 1024                          / (totalDuration / 1000),
                         devicePath);
            UpdateStatus?.Invoke($"Dump finished in {(end - start).TotalSeconds} seconds.");
            UpdateStatus
              ?.Invoke($"Average dump speed {(double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000):F3} KiB/sec.");
            UpdateStatus
              ?.Invoke($"Average write speed {(double)blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration:F3} KiB/sec.");
            dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));
            dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration);

            #region Trimming
            if(resume.BadBlocks.Count > 0 && !aborted && !notrim && newTrim)
            {
                start = DateTime.UtcNow;
                UpdateStatus?.Invoke("Trimming bad sectors");
                dumpLog.WriteLine("Trimming bad sectors");

                ulong[] tmpArray = resume.BadBlocks.ToArray();
                InitProgress?.Invoke();
                foreach(ulong badSector in tmpArray)
                {
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        UpdateStatus?.Invoke("Aborted!");
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    PulseProgress?.Invoke($"Trimming sector {badSector}");

                    sense = scsiReader.ReadBlock(out readBuffer, badSector, out double cmdDuration);

                    if(sense || dev.Error) continue;

                    resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                    outputPlugin.WriteSector(readBuffer, badSector);
                }

                EndProgress?.Invoke();
                end = DateTime.UtcNow;
                UpdateStatus?.Invoke($"Trimmming finished in {(end - start).TotalSeconds} seconds.");
                dumpLog.WriteLine("Trimmming finished in {0} seconds.", (end - start).TotalSeconds);
            }
            #endregion Trimming

            #region Error handling
            if(resume.BadBlocks.Count > 0 && !aborted && retryPasses > 0)
            {
                int  pass              = 1;
                bool forward           = true;
                bool runningPersistent = false;

                Modes.ModePage? currentModePage = null;
                byte[]          md6;
                byte[]          md10;

                if(persistent)
                {
                    Modes.ModePage_01_MMC pgMmc;
                    Modes.ModePage_01     pg;

                    sense = dev.ModeSense6(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                           dev.Timeout, out _);
                    if(sense)
                    {
                        sense = dev.ModeSense10(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                                dev.Timeout, out _);

                        if(!sense)
                        {
                            Modes.DecodedMode? dcMode10 = Modes.DecodeMode10(readBuffer, dev.ScsiType);

                            if(dcMode10.HasValue)
                                foreach(Modes.ModePage modePage in dcMode10.Value.Pages)
                                    if(modePage.Page == 0x01 && modePage.Subpage == 0x00)
                                        currentModePage = modePage;
                        }
                    }
                    else
                    {
                        Modes.DecodedMode? dcMode6 = Modes.DecodeMode6(readBuffer, dev.ScsiType);

                        if(dcMode6.HasValue)
                            foreach(Modes.ModePage modePage in dcMode6.Value.Pages)
                                if(modePage.Page == 0x01 && modePage.Subpage == 0x00)
                                    currentModePage = modePage;
                    }

                    if(currentModePage == null)
                    {
                        if(dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
                        {
                            pgMmc = new Modes.ModePage_01_MMC {PS = false, ReadRetryCount = 32, Parameter = 0x00};
                            currentModePage = new Modes.ModePage
                            {
                                Page = 0x01, Subpage = 0x00, PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                            };
                        }
                        else
                        {
                            pg = new Modes.ModePage_01
                            {
                                PS             = false,
                                AWRE           = true,
                                ARRE           = true,
                                TB             = false,
                                RC             = false,
                                EER            = true,
                                PER            = false,
                                DTE            = true,
                                DCR            = false,
                                ReadRetryCount = 32
                            };

                            currentModePage = new Modes.ModePage
                            {
                                Page = 0x01, Subpage = 0x00, PageResponse = Modes.EncodeModePage_01(pg)
                            };
                        }
                    }

                    if(dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
                    {
                        pgMmc = new Modes.ModePage_01_MMC {PS = false, ReadRetryCount = 255, Parameter = 0x20};
                        Modes.DecodedMode md = new Modes.DecodedMode
                        {
                            Header = new Modes.ModeHeader(),
                            Pages = new[]
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
                        pg = new Modes.ModePage_01
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
                            Pages = new[]
                            {
                                new Modes.ModePage
                                {
                                    Page = 0x01, Subpage = 0x00, PageResponse = Modes.EncodeModePage_01(pg)
                                }
                            }
                        };
                        md6  = Modes.EncodeMode6(md, dev.ScsiType);
                        md10 = Modes.EncodeMode10(md, dev.ScsiType);
                    }

                    UpdateStatus?.Invoke("Sending MODE SELECT to drive (return damaged blocks).");
                    dumpLog.WriteLine("Sending MODE SELECT to drive (return damaged blocks).");
                    sense = dev.ModeSelect(md6, out byte[] senseBuf, true, false, dev.Timeout, out _);
                    if(sense) sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out _);

                    if(sense)
                    {
                        UpdateStatus
                          ?.Invoke("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");
                        DicConsole.DebugWriteLine("Error: {0}", Sense.PrettifySense(senseBuf));
                        dumpLog.WriteLine("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");
                    }
                    else runningPersistent = true;
                }

                InitProgress?.Invoke();
                repeatRetry:
                ulong[] tmpArray = resume.BadBlocks.ToArray();
                foreach(ulong badSector in tmpArray)
                {
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        UpdateStatus?.Invoke("Aborted!");
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    PulseProgress?.Invoke(string.Format("Retrying sector {0}, pass {1}, {3}{2}", badSector, pass,
                                                        forward ? "forward" : "reverse",
                                                        runningPersistent ? "recovering partial data, " : ""));

                    sense         =  scsiReader.ReadBlock(out readBuffer, badSector, out double cmdDuration);
                    totalDuration += cmdDuration;

                    if(!sense && !dev.Error)
                    {
                        resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        outputPlugin.WriteSector(readBuffer, badSector);
                        UpdateStatus?.Invoke($"Correctly retried block {badSector} in pass {pass}.");
                        dumpLog.WriteLine("Correctly retried block {0} in pass {1}.", badSector, pass);
                    }
                    else if(runningPersistent) outputPlugin.WriteSector(readBuffer, badSector);
                }

                if(pass < retryPasses && !aborted && resume.BadBlocks.Count > 0)
                {
                    pass++;
                    forward = !forward;
                    resume.BadBlocks.Sort();
                    resume.BadBlocks.Reverse();
                    goto repeatRetry;
                }

                if(runningPersistent && currentModePage.HasValue)
                {
                    Modes.DecodedMode md = new Modes.DecodedMode
                    {
                        Header = new Modes.ModeHeader(), Pages = new[] {currentModePage.Value}
                    };
                    md6  = Modes.EncodeMode6(md, dev.ScsiType);
                    md10 = Modes.EncodeMode10(md, dev.ScsiType);

                    UpdateStatus?.Invoke("Sending MODE SELECT to drive (return device to previous status).");
                    dumpLog.WriteLine("Sending MODE SELECT to drive (return device to previous status).");
                    sense = dev.ModeSelect(md6, out _, true, false, dev.Timeout, out _);
                    if(sense) dev.ModeSelect10(md10, out _, true, false, dev.Timeout, out _);
                }

                EndProgress?.Invoke();
            }
            #endregion Error handling

            if(!aborted)
                if(opticalDisc)
                    foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
                    {
                        if(tag.Value is null)
                        {
                            DicConsole.ErrorWriteLine("Error: Tag type {0} is null, skipping...", tag.Key);
                            continue;
                        }

                        ret = outputPlugin.WriteMediaTag(tag.Value, tag.Key);

                        if(ret || force) continue;

                        // Cannot write tag to image
                        StoppingErrorMessage?.Invoke($"Cannot write tag {tag.Key}.");
                        dumpLog.WriteLine($"Cannot write tag {tag.Key}." + Environment.NewLine +
                                          outputPlugin.ErrorMessage);
                        return;
                    }
                else
                {
                    if(!dev.IsRemovable || dev.IsUsb)
                    {
                        if(dev.IsUsb && dev.UsbDescriptors != null)
                        {
                            UpdateStatus?.Invoke("Reading USB descriptors.");
                            dumpLog.WriteLine("Reading USB descriptors.");
                            ret = outputPlugin.WriteMediaTag(dev.UsbDescriptors, MediaTagType.USB_Descriptors);

                            if(!ret && !force)
                            {
                                dumpLog.WriteLine("Cannot write USB descriptors.");
                                StoppingErrorMessage?.Invoke("Cannot write USB descriptors." + Environment.NewLine +
                                                             outputPlugin.ErrorMessage);
                                return;
                            }
                        }

                        byte[] cmdBuf;
                        if(dev.Type == DeviceType.ATAPI)
                        {
                            UpdateStatus?.Invoke("Requesting ATAPI IDENTIFY PACKET DEVICE.");
                            dumpLog.WriteLine("Requesting ATAPI IDENTIFY PACKET DEVICE.");
                            sense = dev.AtapiIdentify(out cmdBuf, out _);
                            if(!sense)
                            {
                                ret = outputPlugin.WriteMediaTag(cmdBuf, MediaTagType.ATAPI_IDENTIFY);

                                if(!ret && !force)
                                {
                                    dumpLog.WriteLine("Cannot write ATAPI IDENTIFY PACKET DEVICE.");
                                    StoppingErrorMessage?.Invoke("Cannot write ATAPI IDENTIFY PACKET DEVICE." +
                                                                 Environment.NewLine                          +
                                                                 outputPlugin.ErrorMessage);
                                    return;
                                }
                            }
                        }

                        sense = dev.ScsiInquiry(out cmdBuf, out _);
                        if(!sense)
                        {
                            UpdateStatus?.Invoke("Requesting SCSI INQUIRY.");
                            dumpLog.WriteLine("Requesting SCSI INQUIRY.");
                            ret = outputPlugin.WriteMediaTag(cmdBuf, MediaTagType.SCSI_INQUIRY);

                            if(!ret && !force)
                            {
                                StoppingErrorMessage?.Invoke("Cannot write SCSI INQUIRY.");
                                dumpLog.WriteLine("Cannot write SCSI INQUIRY." + Environment.NewLine +
                                                  outputPlugin.ErrorMessage);
                                return;
                            }

                            UpdateStatus?.Invoke("Requesting MODE SENSE (10).");
                            dumpLog.WriteLine("Requesting MODE SENSE (10).");
                            sense = dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current,
                                                    0x3F, 0xFF, 5, out _);
                            if(!sense || dev.Error)
                                sense = dev.ModeSense10(out cmdBuf, out _, false, true,
                                                        ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out _);

                            decMode = null;

                            if(!sense && !dev.Error)
                                if(Modes.DecodeMode10(cmdBuf, dev.ScsiType).HasValue)
                                {
                                    decMode = Modes.DecodeMode10(cmdBuf, dev.ScsiType);
                                    ret     = outputPlugin.WriteMediaTag(cmdBuf, MediaTagType.SCSI_MODESENSE_10);

                                    if(!ret && !force)
                                    {
                                        dumpLog.WriteLine("Cannot write SCSI MODE SENSE (10).");
                                        StoppingErrorMessage?.Invoke("Cannot write SCSI MODE SENSE (10)." +
                                                                     Environment.NewLine                  +
                                                                     outputPlugin.ErrorMessage);
                                        return;
                                    }
                                }

                            UpdateStatus?.Invoke("Requesting MODE SENSE (6).");
                            dumpLog.WriteLine("Requesting MODE SENSE (6).");
                            sense = dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F,
                                                   0x00, 5, out _);
                            if(sense || dev.Error)
                                sense = dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F,
                                                       0x00, 5, out _);
                            if(sense || dev.Error) sense = dev.ModeSense(out cmdBuf, out _, 5, out _);

                            if(!sense && !dev.Error)
                                if(Modes.DecodeMode6(cmdBuf, dev.ScsiType).HasValue)
                                {
                                    decMode = Modes.DecodeMode6(cmdBuf, dev.ScsiType);
                                    ret     = outputPlugin.WriteMediaTag(cmdBuf, MediaTagType.SCSI_MODESENSE_6);

                                    if(!ret && !force)
                                    {
                                        dumpLog.WriteLine("Cannot write SCSI MODE SENSE (6).");
                                        StoppingErrorMessage?.Invoke("Cannot write SCSI MODE SENSE (6)." +
                                                                     Environment.NewLine                 +
                                                                     outputPlugin.ErrorMessage);
                                        return;
                                    }
                                }
                        }
                    }
                }

            resume.BadBlocks.Sort();
            foreach(ulong bad in resume.BadBlocks) dumpLog.WriteLine("Sector {0} could not be read.", bad);
            currentTry.Extents = ExtentsConverter.ToMetadata(extents);

            outputPlugin.SetDumpHardware(resume.Tries);
            if(preSidecar != null) outputPlugin.SetCicmMetadata(preSidecar);
            dumpLog.WriteLine("Closing output file.");
            UpdateStatus?.Invoke("Closing output file.");
            DateTime closeStart = DateTime.Now;
            outputPlugin.Close();
            DateTime closeEnd = DateTime.Now;
            UpdateStatus?.Invoke($"Closed in {(closeEnd - closeStart).TotalSeconds} seconds.");
            dumpLog.WriteLine("Closed in {0} seconds.", (closeEnd - closeStart).TotalSeconds);

            if(aborted)
            {
                UpdateStatus?.Invoke("Aborted!");
                dumpLog.WriteLine("Aborted!");
                return;
            }

            double totalChkDuration = 0;
            if(!nometadata)
            {
                UpdateStatus?.Invoke("Creating sidecar.");
                dumpLog.WriteLine("Creating sidecar.");
                FiltersList filters     = new FiltersList();
                IFilter     filter      = filters.GetFilter(outputPath);
                IMediaImage inputPlugin = ImageFormat.Detect(filter);
                if(!inputPlugin.Open(filter))
                {
                    StoppingErrorMessage?.Invoke("Could not open created image.");
                    return;
                }

                DateTime chkStart = DateTime.UtcNow;
                sidecarClass                      =  new Sidecar(inputPlugin, outputPath, filter.Id, encoding);
                sidecarClass.InitProgressEvent    += InitProgress;
                sidecarClass.UpdateProgressEvent  += UpdateProgress;
                sidecarClass.EndProgressEvent     += EndProgress;
                sidecarClass.InitProgressEvent2   += InitProgress2;
                sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
                sidecarClass.EndProgressEvent2    += EndProgress2;
                sidecarClass.UpdateStatusEvent    += UpdateStatus;
                CICMMetadataType sidecar = sidecarClass.Create();
                end = DateTime.UtcNow;

                totalChkDuration = (end - chkStart).TotalMilliseconds;
                UpdateStatus?.Invoke($"Sidecar created in {(end - chkStart).TotalSeconds} seconds.");
                UpdateStatus
                  ?.Invoke($"Average checksum speed {(double)blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000):F3} KiB/sec.");
                dumpLog.WriteLine("Sidecar created in {0} seconds.", (end - chkStart).TotalSeconds);
                dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                                  (double)blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                if(opticalDisc)
                {
                    if(preSidecar != null)
                    {
                        preSidecar.OpticalDisc = sidecar.OpticalDisc;
                        sidecar                = preSidecar;
                    }

                    List<(ulong start, string type)> filesystems = new List<(ulong start, string type)>();
                    if(sidecar.OpticalDisc[0].Track != null)
                        filesystems.AddRange(from xmlTrack in sidecar.OpticalDisc[0].Track
                                             where xmlTrack.FileSystemInformation != null
                                             from partition in xmlTrack.FileSystemInformation
                                             where partition.FileSystems != null
                                             from fileSystem in partition.FileSystems
                                             select (partition.StartSector, fileSystem.Type));

                    if(filesystems.Count > 0)
                        foreach(var filesystem in filesystems.Select(o => new {o.start, o.type}).Distinct())
                        {
                            UpdateStatus?.Invoke($"Found filesystem {filesystem.type} at sector {filesystem.start}");
                            dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type, filesystem.start);
                        }

                    // TODO: Implement layers
                    sidecar.OpticalDisc[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);
                    CommonTypes.Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp,
                                                                     out string xmlDskSubTyp);
                    sidecar.OpticalDisc[0].DiscType          = xmlDskTyp;
                    sidecar.OpticalDisc[0].DiscSubType       = xmlDskSubTyp;
                    sidecar.OpticalDisc[0].DumpHardwareArray = resume.Tries.ToArray();

                    foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
                        if(outputPlugin.SupportedMediaTags.Contains(tag.Key))
                            AddMediaTagToSidecar(outputPath, tag, ref sidecar);
                }
                else
                {
                    if(preSidecar != null)
                    {
                        preSidecar.BlockMedia = sidecar.BlockMedia;
                        sidecar               = preSidecar;
                    }

                    // All USB flash drives report as removable, even if the media is not removable
                    if(!dev.IsRemovable || dev.IsUsb)
                    {
                        if(dev.IsUsb)
                            if(outputPlugin.SupportedMediaTags.Contains(MediaTagType.USB_Descriptors))
                                sidecar.BlockMedia[0].USB = new USBType
                                {
                                    ProductID = dev.UsbProductId,
                                    VendorID  = dev.UsbVendorId,
                                    Descriptors = new DumpType
                                    {
                                        Image     = outputPath,
                                        Size      = (ulong)dev.UsbDescriptors.Length,
                                        Checksums = Checksum.GetChecksums(dev.UsbDescriptors).ToArray()
                                    }
                                };

                        byte[] cmdBuf;
                        if(dev.Type == DeviceType.ATAPI)
                        {
                            sense = dev.AtapiIdentify(out cmdBuf, out _);
                            if(!sense)
                                if(outputPlugin.SupportedMediaTags.Contains(MediaTagType.ATAPI_IDENTIFY))
                                    sidecar.BlockMedia[0].ATA = new ATAType
                                    {
                                        Identify = new DumpType
                                        {
                                            Image     = outputPath,
                                            Size      = (ulong)cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                        }
                                    };
                        }

                        sense = dev.ScsiInquiry(out cmdBuf, out _);
                        if(!sense)
                        {
                            if(outputPlugin.SupportedMediaTags.Contains(MediaTagType.SCSI_INQUIRY))
                                sidecar.BlockMedia[0].SCSI = new SCSIType
                                {
                                    Inquiry = new DumpType
                                    {
                                        Image     = outputPath,
                                        Size      = (ulong)cmdBuf.Length,
                                        Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                    }
                                };

                            // TODO: SCSI Extended Vendor Page descriptors
                            /*
                            UpdateStatus?.Invoke("Reading SCSI Extended Vendor Page Descriptors.");
                            dumpLog.WriteLine("Reading SCSI Extended Vendor Page Descriptors.");
                            sense = dev.ScsiInquiry(out cmdBuf, out _, 0x00);
                            if(!sense)
                            {
                                byte[] pages = EVPD.DecodePage00(cmdBuf);

                                if(pages != null)
                                {
                                    List<EVPDType> evpds = new List<EVPDType>();
                                    foreach(byte page in pages)
                                    {
                                        dumpLog.WriteLine("Requesting page {0:X2}h.", page);
                                        sense = dev.ScsiInquiry(out cmdBuf, out _, page);
                                        if(sense) continue;

                                        EVPDType evpd = new EVPDType
                                        {
                                            Image = $"{outputPrefix}.evpd_{page:X2}h.bin",
                                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray(),
                                            Size = cmdBuf.Length
                                        };
                                        evpd.Checksums = Checksum.GetChecksums(cmdBuf).ToArray();
                                        DataFile.WriteTo("SCSI Dump", evpd.Image, cmdBuf);
                                        evpds.Add(evpd);
                                    }

                                    if(evpds.Count > 0) sidecar.BlockMedia[0].SCSI.EVPD = evpds.ToArray();
                                }
                            }
                            */

                            UpdateStatus?.Invoke("Requesting MODE SENSE (10).");
                            dumpLog.WriteLine("Requesting MODE SENSE (10).");
                            sense = dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current,
                                                    0x3F, 0xFF, 5, out _);
                            if(!sense || dev.Error)
                                sense = dev.ModeSense10(out cmdBuf, out _, false, true,
                                                        ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out _);

                            decMode = null;

                            if(!sense && !dev.Error)
                                if(Modes.DecodeMode10(cmdBuf, dev.ScsiType).HasValue)
                                    if(outputPlugin.SupportedMediaTags.Contains(MediaTagType.SCSI_MODESENSE_10))
                                        sidecar.BlockMedia[0].SCSI.ModeSense10 = new DumpType
                                        {
                                            Image     = outputPath,
                                            Size      = (ulong)cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                        };

                            UpdateStatus?.Invoke("Requesting MODE SENSE (6).");
                            dumpLog.WriteLine("Requesting MODE SENSE (6).");
                            sense = dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F,
                                                   0x00, 5, out _);
                            if(sense || dev.Error)
                                sense = dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F,
                                                       0x00, 5, out _);
                            if(sense || dev.Error) sense = dev.ModeSense(out cmdBuf, out _, 5, out _);

                            if(!sense && !dev.Error)
                                if(Modes.DecodeMode6(cmdBuf, dev.ScsiType).HasValue)
                                    if(outputPlugin.SupportedMediaTags.Contains(MediaTagType.SCSI_MODESENSE_6))
                                        sidecar.BlockMedia[0].SCSI.ModeSense = new DumpType
                                        {
                                            Image     = outputPath,
                                            Size      = (ulong)cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                        };
                        }
                    }

                    List<(ulong start, string type)> filesystems = new List<(ulong start, string type)>();
                    if(sidecar.BlockMedia[0].FileSystemInformation != null)
                        filesystems.AddRange(from partition in sidecar.BlockMedia[0].FileSystemInformation
                                             where partition.FileSystems != null
                                             from fileSystem in partition.FileSystems
                                             select (partition.StartSector, fileSystem.Type));

                    if(filesystems.Count > 0)
                        foreach(var filesystem in filesystems.Select(o => new {o.start, o.type}).Distinct())
                        {
                            UpdateStatus?.Invoke($"Found filesystem {filesystem.type} at sector {filesystem.start}");
                            dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type, filesystem.start);
                        }

                    sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);
                    CommonTypes.Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp,
                                                                     out string xmlDskSubTyp);
                    sidecar.BlockMedia[0].DiskType    = xmlDskTyp;
                    sidecar.BlockMedia[0].DiskSubType = xmlDskSubTyp;
                    // TODO: Implement device firmware revision
                    if(!dev.IsRemovable || dev.IsUsb)
                        if(dev.Type == DeviceType.ATAPI) sidecar.BlockMedia[0].Interface = "ATAPI";
                        else if(dev.IsUsb) sidecar.BlockMedia[0].Interface               = "USB";
                        else if(dev.IsFireWire) sidecar.BlockMedia[0].Interface          = "FireWire";
                        else sidecar.BlockMedia[0].Interface                             = "SCSI";
                    sidecar.BlockMedia[0].LogicalBlocks     = blocks;
                    sidecar.BlockMedia[0].PhysicalBlockSize = physicalBlockSize;
                    sidecar.BlockMedia[0].LogicalBlockSize  = logicalBlockSize;
                    sidecar.BlockMedia[0].Manufacturer      = dev.Manufacturer;
                    sidecar.BlockMedia[0].Model             = dev.Model;
                    sidecar.BlockMedia[0].Serial            = dev.Serial;
                    sidecar.BlockMedia[0].Size              = blocks * blockSize;

                    if(dev.IsRemovable) sidecar.BlockMedia[0].DumpHardwareArray = resume.Tries.ToArray();
                }

                UpdateStatus?.Invoke("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml", FileMode.Create);

                XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }

            UpdateStatus?.Invoke("");
            UpdateStatus
              ?.Invoke($"Took a total of {(end - start).TotalSeconds:F3} seconds ({totalDuration / 1000:F3} processing commands, {totalChkDuration / 1000:F3} checksumming, {imageWriteDuration:F3} writing, {(closeEnd - closeStart).TotalSeconds:F3} closing).");
            UpdateStatus
              ?.Invoke($"Average speed: {(double)blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000):F3} MiB/sec.");
            UpdateStatus?.Invoke($"Fastest speed burst: {maxSpeed:F3} MiB/sec.");
            UpdateStatus?.Invoke($"Slowest speed burst: {minSpeed:F3} MiB/sec.");
            UpdateStatus?.Invoke($"{resume.BadBlocks.Count} sectors could not be read.");
            UpdateStatus?.Invoke("");

            Statistics.AddMedia(dskType, true);
        }
    }
}