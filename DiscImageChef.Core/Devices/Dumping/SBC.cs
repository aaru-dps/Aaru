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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;
using DiscImageChef.Metadata;
using Extents;
using Schemas;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using TrackType = DiscImageChef.DiscImages.TrackType;

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>
    ///     Implements dumping SCSI Block Commands and Reduced Block Commands devices
    /// </summary>
    static class Sbc
    {
        /// <summary>
        ///     Dumps a SCSI Block Commands device or a Reduced Block Commands devices
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="outputPrefix">Prefix for output data files</param>
        /// <param name="outputPlugin">Plugin for output file</param>
        /// <param name="retryPasses">How many times to retry</param>
        /// <param name="force">Force to continue dump whenever possible</param>
        /// <param name="dumpRaw">Dump long or scrambled sectors</param>
        /// <param name="persistent">Store whatever data the drive returned on error</param>
        /// <param name="stopOnError">Stop dump on first error</param>
        /// <param name="resume">Information for dump resuming</param>
        /// <param name="dumpLog">Dump logger</param>
        /// <param name="encoding">Encoding to use when analyzing dump</param>
        /// <param name="opticalDisc">If device contains an optical disc (e.g. DVD or BD)</param>
        /// <param name="mediaTags">Media tags as retrieved in MMC layer</param>
        /// <param name="dskType">Disc type as detected in SCSI or MMC layer</param>
        /// <param name="outputPath">Path to output file</param>
        /// <param name="formatOptions">Formats to pass to output file plugin</param>
        /// <exception cref="InvalidOperationException">If the resume file is invalid</exception>
        internal static void Dump(Device                           dev,          string        devicePath,
                                  IWritableImage                   outputPlugin, ushort        retryPasses,
                                  bool                             force,        bool          dumpRaw,
                                  bool                             persistent,   bool          stopOnError,
                                  Dictionary<MediaTagType, byte[]> mediaTags,    ref MediaType dskType,
                                  bool                             opticalDisc,
                                  ref Resume                       resume,     ref DumpLog                dumpLog,
                                  Encoding                         encoding,   string                     outputPrefix,
                                  string                           outputPath, Dictionary<string, string> formatOptions,
                                  CICMMetadataType                 preSidecar, uint                       skip,
                                  bool                             nometadata)
        {
            bool         sense;
            ulong        blocks;
            uint         blockSize;
            uint         logicalBlockSize;
            uint         physicalBlockSize;
            byte         scsiMediumType     = 0;
            byte         scsiDensityCode    = 0;
            bool         containsFloppyPage = false;
            const ushort SBC_PROFILE        = 0x0001;
            DateTime     start;
            DateTime     end;
            double       totalDuration = 0;
            double       currentSpeed  = 0;
            double       maxSpeed      = double.MinValue;
            double       minSpeed      = double.MaxValue;
            byte[]       readBuffer;
            uint         blocksToRead;
            bool         aborted = false;
            System.Console.CancelKeyPress += (sender, e) => e.Cancel = aborted = true;
            Modes.DecodedMode? decMode = null;

            dumpLog.WriteLine("Initializing reader.");
            Reader scsiReader = new Reader(dev, dev.Timeout, null, dumpRaw);
            blocks    = scsiReader.GetDeviceBlocks();
            blockSize = scsiReader.LogicalBlockSize;
            if(scsiReader.FindReadCommand())
            {
                dumpLog.WriteLine("ERROR: Cannot find correct read command: {0}.", scsiReader.ErrorMessage);
                DicConsole.ErrorWriteLine("Unable to read medium.");
                return;
            }

            if(blocks != 0 && blockSize != 0)
            {
                blocks++;
                DicConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2} bytes)", blocks,
                                     blockSize, blocks * (ulong)blockSize);
            }

            // Check how many blocks to read, if error show and return
            if(scsiReader.GetBlocksToRead())
            {
                dumpLog.WriteLine("ERROR: Cannot get blocks to read: {0}.", scsiReader.ErrorMessage);
                DicConsole.ErrorWriteLine(scsiReader.ErrorMessage);
                return;
            }

            blocksToRead      = scsiReader.BlocksToRead;
            logicalBlockSize  = blockSize;
            physicalBlockSize = scsiReader.PhysicalBlockSize;

            if(blocks == 0)
            {
                dumpLog.WriteLine("ERROR: Unable to read medium or empty medium present...");
                DicConsole.ErrorWriteLine("Unable to read medium or empty medium present...");
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

            dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).",       blocks, blocks * blockSize);
            dumpLog.WriteLine("Device can read {0} blocks at a time.",        blocksToRead);
            dumpLog.WriteLine("Device reports {0} bytes per logical block.",  blockSize);
            dumpLog.WriteLine("Device reports {0} bytes per physical block.", scsiReader.LongBlockSize);
            dumpLog.WriteLine("SCSI device type: {0}.",                       dev.ScsiType);
            dumpLog.WriteLine("SCSI medium type: {0}.",                       scsiMediumType);
            dumpLog.WriteLine("SCSI density type: {0}.",                      scsiDensityCode);

            if(dskType == MediaType.Unknown && dev.IsUsb && containsFloppyPage) dskType = MediaType.FlashDrive;

            DicConsole.WriteLine("Media identified as {0}", dskType);
            dumpLog.WriteLine("SCSI floppy mode page present: {0}.", containsFloppyPage);
            dumpLog.WriteLine("Media identified as {0}.",            dskType);

            uint longBlockSize = scsiReader.LongBlockSize;

            if(dumpRaw)
                if(blockSize == longBlockSize)
                {
                    DicConsole.ErrorWriteLine(!scsiReader.CanReadRaw
                                                  ? "Device doesn't seem capable of reading raw data from media."
                                                  : "Device is capable of reading raw data but I've been unable to guess correct sector size.");

                    if(!force)
                    {
                        DicConsole
                           .ErrorWriteLine("Not continuing. If you want to continue reading cooked data when raw is not available use the force option.");
                        // TODO: Exit more gracefully
                        return;
                    }

                    DicConsole.ErrorWriteLine("Continuing dumping cooked data.");
                    dumpRaw = false;
                }
                else
                {
                    // Only a block will be read, but it contains 16 sectors and command expect sector number not block number
                    blocksToRead = (uint)(longBlockSize == 37856 ? 16 : 1);
                    DicConsole.WriteLine("Reading {0} raw bytes ({1} cooked bytes) per sector.", longBlockSize,
                                         blockSize * blocksToRead);
                    physicalBlockSize = longBlockSize;
                    blockSize         = longBlockSize;
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

            DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

            MhddLog mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
            IbgLog  ibgLog  = new IbgLog(outputPrefix  + ".ibg", SBC_PROFILE);
            ret = outputPlugin.Create(outputPath, dskType, formatOptions, blocks, blockSize);

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
            double imageWriteDuration = 0;

            if(opticalDisc)
                outputPlugin.SetTracks(new List<Track>
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
                        DicConsole.WriteLine("Setting geometry to {0} cylinders, {1} heads, {2} sectors per track",
                                             rigidPage.Value.Cylinders, rigidPage.Value.Heads,
                                             (uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads)));
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
                        DicConsole.WriteLine("Setting geometry to {0} cylinders, {1} heads, {2} sectors per track",
                                             flexiblePage.Value.Cylinders, flexiblePage.Value.Heads,
                                             flexiblePage.Value.SectorsPerTrack);
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
                throw new InvalidOperationException("Could not process resume file, not continuing...");

            if(resume.NextBlock > 0) dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);

            for(ulong i = resume.NextBlock; i < blocks; i += blocksToRead)
            {
                if(aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    dumpLog.WriteLine("Aborted!");
                    break;
                }

                if(blocks - i < blocksToRead) blocksToRead = (uint)(blocks - i);

                #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

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
                    i += skip - blocksToRead;
                }

                double newSpeed =
                    (double)blockSize * blocksToRead / 1048576 / (cmdDuration / 1000);
                if(!double.IsInfinity(newSpeed)) currentSpeed = newSpeed;
                resume.NextBlock = i + blocksToRead;
            }

            end = DateTime.UtcNow;
            DicConsole.WriteLine();
            mhddLog.Close();
            ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                         blockSize * (double)(blocks + 1) / 1024                          / (totalDuration / 1000),
                         devicePath);
            dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));
            dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration);

            #region Error handling
            if(resume.BadBlocks.Count > 0 && !aborted && retryPasses > 0)
            {
                int  pass              = 0;
                bool forward           = true;
                bool runningPersistent = false;

                Modes.ModePage? currentModePage = null;
                byte[]          md6;
                byte[]          md10;

                if(persistent)
                {
                    if(dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
                    {
                        Modes.ModePage_01_MMC pgMmc =
                            new Modes.ModePage_01_MMC {PS = false, ReadRetryCount = 255, Parameter = 0x20};
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
                            Pages = new[]
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
                    sense = dev.ModeSelect(md6, out byte[] senseBuf, true, false, dev.Timeout, out _);
                    if(sense) sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out _);

                    if(sense)
                    {
                        DicConsole
                           .WriteLine("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");
                        DicConsole.DebugWriteLine("Error: {0}", Sense.PrettifySense(senseBuf));
                        dumpLog.WriteLine("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");
                    }
                    else runningPersistent = true;
                }

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

                    sense         =  scsiReader.ReadBlock(out readBuffer, badSector, out double cmdDuration);
                    totalDuration += cmdDuration;

                    if(!sense && !dev.Error)
                    {
                        resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        outputPlugin.WriteSector(readBuffer, badSector);
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
                        Header = new Modes.ModeHeader(),
                        Pages  = new[] {currentModePage.Value}
                    };
                    md6  = Modes.EncodeMode6(md, dev.ScsiType);
                    md10 = Modes.EncodeMode10(md, dev.ScsiType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive.");
                    sense = dev.ModeSelect(md6, out _, true, false, dev.Timeout, out _);
                    if(sense) dev.ModeSelect10(md10, out _, true, false, dev.Timeout, out _);
                }

                DicConsole.WriteLine();
            }
            #endregion Error handling

            if(!aborted)
                if(opticalDisc)
                    foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
                    {
                        ret = outputPlugin.WriteMediaTag(tag.Value, tag.Key);

                        if(ret || force) continue;

                        // Cannot write tag to image
                        dumpLog.WriteLine($"Cannot write tag {tag.Key}.");
                        throw new ArgumentException(outputPlugin.ErrorMessage);
                    }
                else
                {
                    if(!dev.IsRemovable || dev.IsUsb)
                    {
                        if(dev.IsUsb)
                        {
                            dumpLog.WriteLine("Reading USB descriptors.");
                            ret = outputPlugin.WriteMediaTag(dev.UsbDescriptors, MediaTagType.USB_Descriptors);

                            if(!ret && !force)
                            {
                                dumpLog.WriteLine($"Cannot write USB descriptors.");
                                throw new ArgumentException(outputPlugin.ErrorMessage);
                            }
                        }

                        byte[] cmdBuf;
                        if(dev.Type == DeviceType.ATAPI)
                        {
                            dumpLog.WriteLine("Requesting ATAPI IDENTIFY PACKET DEVICE.");
                            sense = dev.AtapiIdentify(out cmdBuf, out _);
                            if(!sense)
                            {
                                ret = outputPlugin.WriteMediaTag(cmdBuf, MediaTagType.ATAPI_IDENTIFY);

                                if(!ret && !force)
                                {
                                    dumpLog.WriteLine($"Cannot write ATAPI IDENTIFY PACKET DEVICE.");
                                    throw new ArgumentException(outputPlugin.ErrorMessage);
                                }
                            }
                        }

                        sense = dev.ScsiInquiry(out cmdBuf, out _);
                        if(!sense)
                        {
                            dumpLog.WriteLine("Requesting SCSI INQUIRY.");
                            ret = outputPlugin.WriteMediaTag(cmdBuf, MediaTagType.SCSI_INQUIRY);

                            if(!ret && !force)
                            {
                                dumpLog.WriteLine($"Cannot write SCSI INQUIRY.");
                                throw new ArgumentException(outputPlugin.ErrorMessage);
                            }

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
                                        dumpLog.WriteLine($"Cannot write SCSI MODE SENSE (10).");
                                        throw new ArgumentException(outputPlugin.ErrorMessage);
                                    }
                                }

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
                                        dumpLog.WriteLine($"Cannot write SCSI MODE SENSE (6).");
                                        throw new ArgumentException(outputPlugin.ErrorMessage);
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
            DicConsole.WriteLine("Closing output file.");
            DateTime closeStart = DateTime.Now;
            outputPlugin.Close();
            DateTime closeEnd = DateTime.Now;
            dumpLog.WriteLine("Closed in {0} seconds.", (closeEnd - closeStart).TotalSeconds);

            if(aborted)
            {
                dumpLog.WriteLine("Aborted!");
                return;
            }

            double totalChkDuration = 0;
            if(!nometadata)
            {
                dumpLog.WriteLine("Creating sidecar.");
                FiltersList filters     = new FiltersList();
                IFilter     filter      = filters.GetFilter(outputPath);
                IMediaImage inputPlugin = ImageFormat.Detect(filter);
                if(!inputPlugin.Open(filter)) throw new ArgumentException("Could not open created image.");

                DateTime         chkStart = DateTime.UtcNow;
                CICMMetadataType sidecar  = Sidecar.Create(inputPlugin, outputPath, filter.Id, encoding);
                end = DateTime.UtcNow;

                totalChkDuration = (end - chkStart).TotalMilliseconds;
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
                                             select ((ulong)partition.StartSector, fileSystem.Type));

                    if(filesystems.Count > 0)
                        foreach(var filesystem in filesystems.Select(o => new {o.start, o.type}).Distinct())
                            dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type, filesystem.start);
                    // TODO: Implement layers
                    sidecar.OpticalDisc[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);
                    Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp, out string xmlDskSubTyp);
                    sidecar.OpticalDisc[0].DiscType          = xmlDskTyp;
                    sidecar.OpticalDisc[0].DiscSubType       = xmlDskSubTyp;
                    sidecar.OpticalDisc[0].DumpHardwareArray = resume.Tries.ToArray();

                    foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
                        if(outputPlugin.SupportedMediaTags.Contains(tag.Key))
                            Mmc.AddMediaTagToSidecar(outputPath, tag, ref sidecar);
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
                                        Size      = dev.UsbDescriptors.Length,
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
                                            Size      = cmdBuf.Length,
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
                                        Size      = cmdBuf.Length,
                                        Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                    }
                                };

                            // TODO: SCSI Extended Vendor Page descriptors
                            /*
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
                                            Size      = cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                        };

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
                                            Size      = cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                        };
                        }
                    }

                    List<(ulong start, string type)> filesystems = new List<(ulong start, string type)>();
                    if(sidecar.BlockMedia[0].FileSystemInformation != null)
                        filesystems.AddRange(from partition in sidecar.BlockMedia[0].FileSystemInformation
                                             where partition.FileSystems != null
                                             from fileSystem in partition.FileSystems
                                             select ((ulong)partition.StartSector, fileSystem.Type));

                    if(filesystems.Count > 0)
                        foreach(var filesystem in filesystems.Select(o => new {o.start, o.type}).Distinct())
                            dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type, filesystem.start);
                    sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);
                    Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp, out string xmlDskSubTyp);
                    sidecar.BlockMedia[0].DiskType    = xmlDskTyp;
                    sidecar.BlockMedia[0].DiskSubType = xmlDskSubTyp;
                    // TODO: Implement device firmware revision
                    if(!dev.IsRemovable || dev.IsUsb)
                        if(dev.Type == DeviceType.ATAPI)
                            sidecar.BlockMedia[0].Interface                     = "ATAPI";
                        else if(dev.IsUsb) sidecar.BlockMedia[0].Interface      = "USB";
                        else if(dev.IsFireWire) sidecar.BlockMedia[0].Interface = "FireWire";
                        else sidecar.BlockMedia[0].Interface                    = "SCSI";
                    sidecar.BlockMedia[0].LogicalBlocks     = (long)blocks;
                    sidecar.BlockMedia[0].PhysicalBlockSize = (int)physicalBlockSize;
                    sidecar.BlockMedia[0].LogicalBlockSize  = (int)logicalBlockSize;
                    sidecar.BlockMedia[0].Manufacturer      = dev.Manufacturer;
                    sidecar.BlockMedia[0].Model             = dev.Model;
                    sidecar.BlockMedia[0].Serial            = dev.Serial;
                    sidecar.BlockMedia[0].Size              = (long)(blocks * blockSize);

                    if(dev.IsRemovable) sidecar.BlockMedia[0].DumpHardwareArray = resume.Tries.ToArray();
                }

                DicConsole.WriteLine("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml", FileMode.Create);

                XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took a total of {0:F3} seconds ({1:F3} processing commands, {2:F3} checksumming, {3:F3} writing, {4:F3} closing).",
                                 (end - start).TotalSeconds, totalDuration / 1000,
                                 totalChkDuration                          / 1000,
                                 imageWriteDuration, (closeEnd - closeStart).TotalSeconds);
            DicConsole.WriteLine("Avegare speed: {0:F3} MiB/sec.",
                                 (double)blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000));
            DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", maxSpeed);
            DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", minSpeed);
            DicConsole.WriteLine("{0} sectors could not be read.",       resume.BadBlocks.Count);
            DicConsole.WriteLine();

            Statistics.AddMedia(dskType, true);
        }
    }
}