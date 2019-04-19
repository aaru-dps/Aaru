using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Version = DiscImageChef.CommonTypes.Interop.Version;

namespace DiscImageChef.Core.Devices.Dumping
{
    public static class PlayStationPortable
    {
        static readonly byte[] FatSignature = {0x46, 0x41, 0x54, 0x31, 0x36, 0x20, 0x20, 0x20};
        static readonly byte[] IsoExtension = {0x49, 0x53, 0x4F};

        /// <summary>
        ///     Dumps a CFW PlayStation Portable UMD
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="outputPrefix">Prefix for output data files</param>
        /// <param name="outputPlugin">Plugin for output file</param>
        /// <param name="retryPasses">How many times to retry</param>
        /// <param name="force">Force to continue dump whenever possible</param>
        /// <param name="persistent">Store whatever data the drive returned on error</param>
        /// <param name="stopOnError">Stop dump on first error</param>
        /// <param name="resume">Information for dump resuming</param>
        /// <param name="dumpLog">Dump logger</param>
        /// <param name="encoding">Encoding to use when analyzing dump</param>
        /// <param name="outputPath">Path to output file</param>
        /// <param name="formatOptions">Formats to pass to output file plugin</param>
        /// <param name="preSidecar">Existing sidecar</param>
        /// <param name="skip">How many sectors to skip on errors</param>
        /// <param name="nometadata">Don't create metadata sidecar</param>
        /// <param name="notrim">Don't trim errors</param>
        /// <exception cref="ArgumentException">If you asked to dump long sectors from a SCSI Streaming device</exception>
        public static void Dump(Device                     dev,          string     devicePath,
                                IWritableImage             outputPlugin, ushort     retryPasses,
                                bool                       force,        bool       persistent,
                                bool                       stopOnError,  ref Resume resume, ref DumpLog dumpLog,
                                Encoding                   encoding,     string     outputPrefix,
                                string                     outputPath,
                                Dictionary<string, string> formatOptions, CICMMetadataType preSidecar,
                                uint                       skip,
                                bool                       nometadata, bool notrim)
        {
            if(!outputPlugin.SupportedMediaTypes.Contains(MediaType.MemoryStickDuo)    &&
               !outputPlugin.SupportedMediaTypes.Contains(MediaType.MemoryStickProDuo) &&
               !outputPlugin.SupportedMediaTypes.Contains(MediaType.UMD))
            {
                DicConsole.WriteLine("Selected output plugin does not support MemoryStick Duo or UMD, cannot dump...");
                dumpLog.WriteLine("Selected output plugin does not support MemoryStick Duo or UMD, cannot dump...");
                return;
            }

            DicConsole.WriteLine("Checking if media is UMD or MemoryStick...");
            dumpLog.WriteLine("Checking if media is UMD or MemoryStick...");

            bool sense = dev.ModeSense6(out byte[] buffer, out _, false, ScsiModeSensePageControl.Current, 0,
                                        dev.Timeout, out _);

            if(sense)
            {
                DicConsole.WriteLine("Could not get MODE SENSE...");
                dumpLog.WriteLine("Could not get MODE SENSE...");
                return;
            }

            Modes.DecodedMode? decoded = Modes.DecodeMode6(buffer, PeripheralDeviceTypes.DirectAccess);

            if(!decoded.HasValue)
            {
                DicConsole.WriteLine("Could not decode MODE SENSE...");
                dumpLog.WriteLine("Could not decode MODE SENSE...");
                return;
            }

            // UMDs are always write protected
            if(!decoded.Value.Header.WriteProtected)
            {
                DumpMs(dev, devicePath, outputPlugin, retryPasses, force, persistent, stopOnError,
                       ref resume,
                       ref dumpLog, encoding, outputPrefix, outputPath, formatOptions, preSidecar, skip,
                       nometadata,
                       notrim);
                return;
            }

            sense = dev.Read12(out buffer, out _, 0, false, true, false, false, 0, 512, 0, 1, false, dev.Timeout,
                               out _);

            if(sense)
            {
                DicConsole.WriteLine("Could not read...");
                dumpLog.WriteLine("Could not read...");
                return;
            }

            byte[] tmp = new byte[8];

            Array.Copy(buffer, 0x36, tmp, 0, 8);

            // UMDs are stored inside a FAT16 volume
            if(!tmp.SequenceEqual(FatSignature))
            {
                DumpMs(dev, devicePath, outputPlugin, retryPasses, force, persistent, stopOnError,
                       ref resume,
                       ref dumpLog, encoding, outputPrefix, outputPath, formatOptions, preSidecar, skip,
                       nometadata,
                       notrim);
                return;
            }

            ushort fatStart      = (ushort)((buffer[0x0F] << 8) + buffer[0x0E]);
            ushort sectorsPerFat = (ushort)((buffer[0x17] << 8) + buffer[0x16]);
            ushort rootStart     = (ushort)(sectorsPerFat * 2   + fatStart);

            DicConsole.WriteLine("Reading root directory in sector {0}...", rootStart);
            dumpLog.WriteLine("Reading root directory in sector {0}...", rootStart);

            sense = dev.Read12(out buffer, out _, 0, false, true, false, false, rootStart, 512, 0, 1, false,
                               dev.Timeout, out _);

            if(sense)
            {
                DicConsole.WriteLine("Could not read...");
                dumpLog.WriteLine("Could not read...");
                return;
            }

            tmp = new byte[3];
            Array.Copy(buffer, 0x28, tmp, 0, 3);

            if(!tmp.SequenceEqual(IsoExtension))
            {
                DumpMs(dev, devicePath, outputPlugin, retryPasses, force, persistent, stopOnError,
                       ref resume,
                       ref dumpLog, encoding, outputPrefix, outputPath, formatOptions, preSidecar, skip,
                       nometadata,
                       notrim);
                return;
            }

            DicConsole.WriteLine("FAT starts at sector {0} and runs for {1} sectors...", fatStart, sectorsPerFat);
            dumpLog.WriteLine("FAT starts at sector {0} and runs for {1} sectors...", fatStart, sectorsPerFat);

            DicConsole.WriteLine("Reading FAT...");
            dumpLog.WriteLine("Reading FAT...");

            byte[] fat = new byte[sectorsPerFat * 512];

            uint position = 0;

            while(position < sectorsPerFat)
            {
                uint transfer                                    = 64;
                if(transfer + position > sectorsPerFat) transfer = sectorsPerFat - position;
                sense = dev.Read12(out buffer, out _, 0, false, true, false, false, position + fatStart, 512, 0,
                                   transfer, false, dev.Timeout, out _);

                if(sense)
                {
                    DicConsole.WriteLine("Could not read...");
                    dumpLog.WriteLine("Could not read...");
                    return;
                }

                Array.Copy(buffer, 0, fat, position * 512, transfer * 512);

                position += transfer;
            }

            DicConsole.WriteLine("Traversing FAT...");
            dumpLog.WriteLine("Traversing FAT...");

            ushort previousCluster = BitConverter.ToUInt16(fat, 4);

            for(int i = 3; i < fat.Length / 2; i++)
            {
                ushort nextCluster = BitConverter.ToUInt16(fat, i * 2);

                if(nextCluster == previousCluster + 1)
                {
                    previousCluster = nextCluster;
                    continue;
                }

                if(nextCluster == 0xFFFF) break;

                DumpMs(dev, devicePath, outputPlugin, retryPasses, force, persistent, stopOnError,
                       ref resume,
                       ref dumpLog, encoding, outputPrefix, outputPath, formatOptions, preSidecar, skip,
                       nometadata,
                       notrim);
                return;
            }

            if(outputPlugin is IWritableOpticalImage opticalPlugin)
                DumpUmd(dev, devicePath, opticalPlugin, retryPasses, force, persistent, stopOnError,
                        ref resume,
                        ref dumpLog, encoding, outputPrefix, outputPath, formatOptions, preSidecar, skip,
                        nometadata,
                        notrim);
            else DicConsole.ErrorWriteLine("The specified plugin does not support storing optical disc images.");
        }

        static void DumpUmd(Device                     dev,          string     devicePath,
                            IWritableOpticalImage      outputPlugin, ushort     retryPasses,
                            bool                       force,        bool       persistent,
                            bool                       stopOnError,  ref Resume resume, ref DumpLog dumpLog,
                            Encoding                   encoding,     string     outputPrefix,
                            string                     outputPath,
                            Dictionary<string, string> formatOptions, CICMMetadataType preSidecar,
                            uint                       skip,
                            bool                       nometadata, bool notrim)
        {
            const uint      BLOCK_SIZE    = 2048;
            const MediaType DSK_TYPE      = MediaType.UMD;
            uint            blocksToRead  = 16;
            double          totalDuration = 0;
            double          currentSpeed  = 0;
            double          maxSpeed      = double.MinValue;
            double          minSpeed      = double.MaxValue;
            bool            aborted       = false;
            DateTime        start;
            DateTime        end;
            System.Console.CancelKeyPress += (sender, e) => e.Cancel = aborted = true;

            bool sense = dev.Read12(out byte[] readBuffer, out _, 0, false, true, false, false, 0, 512, 0, 1, false,
                                    dev.Timeout, out _);

            if(sense)
            {
                DicConsole.WriteLine("Could not read...");
                dumpLog.WriteLine("Could not read...");
                return;
            }

            ushort fatStart      = (ushort)((readBuffer[0x0F] << 8)                          + readBuffer[0x0E]);
            ushort sectorsPerFat = (ushort)((readBuffer[0x17] << 8)                          + readBuffer[0x16]);
            ushort rootStart     = (ushort)(sectorsPerFat                                * 2 + fatStart);
            ushort rootSize      = (ushort)(((readBuffer[0x12] << 8) + readBuffer[0x11]) * 32 / 512);
            ushort umdStart      = (ushort)(rootStart + rootSize);

            DicConsole.WriteLine("Reading root directory in sector {0}...", rootStart);
            dumpLog.WriteLine("Reading root directory in sector {0}...", rootStart);

            sense = dev.Read12(out readBuffer, out _, 0, false, true, false, false, rootStart, 512, 0, 1, false,
                               dev.Timeout, out _);

            if(sense)
            {
                DicConsole.WriteLine("Could not read...");
                dumpLog.WriteLine("Could not read...");
                return;
            }

            uint   umdSizeInBytes  = BitConverter.ToUInt32(readBuffer, 0x3C);
            ulong  blocks          = umdSizeInBytes / BLOCK_SIZE;
            string mediaPartNumber = Encoding.ASCII.GetString(readBuffer, 0, 11).Trim();

            DicConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2} bytes)", blocks,
                                 BLOCK_SIZE, blocks * (ulong)BLOCK_SIZE);

            dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).",       blocks, blocks * BLOCK_SIZE);
            dumpLog.WriteLine("Device can read {0} blocks at a time.",        blocksToRead);
            dumpLog.WriteLine("Device reports {0} bytes per logical block.",  BLOCK_SIZE);
            dumpLog.WriteLine("Device reports {0} bytes per physical block.", 2048);
            dumpLog.WriteLine("SCSI device type: {0}.",                       dev.ScsiType);
            dumpLog.WriteLine("Media identified as {0}.",                     DSK_TYPE);
            dumpLog.WriteLine("Media part number is {0}.",                    mediaPartNumber);
            DicConsole.WriteLine("Media identified as {0}",  DSK_TYPE);
            DicConsole.WriteLine("Media part number is {0}", mediaPartNumber);

            bool ret;

            DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

            MhddLog mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, BLOCK_SIZE, blocksToRead);
            IbgLog  ibgLog  = new IbgLog(outputPrefix  + ".ibg", 0x0010);
            ret = outputPlugin.Create(outputPath, DSK_TYPE, formatOptions, blocks, BLOCK_SIZE);

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

            outputPlugin.SetTracks(new List<Track>
            {
                new Track
                {
                    TrackBytesPerSector    = (int)BLOCK_SIZE,
                    TrackEndSector         = blocks - 1,
                    TrackSequence          = 1,
                    TrackRawBytesPerSector = (int)BLOCK_SIZE,
                    TrackSubchannelType    = TrackSubchannelType.None,
                    TrackSession           = 1,
                    TrackType              = TrackType.Data
                }
            });

            DumpHardwareType currentTry = null;
            ExtentsULong     extents    = null;
            ResumeSupport.Process(true, dev.IsRemovable, blocks, dev.Manufacturer, dev.Model, dev.Serial,
                                  dev.PlatformId, ref resume, ref currentTry, ref extents);
            if(currentTry == null || extents == null)
                throw new InvalidOperationException("Could not process resume file, not continuing...");

            if(resume.NextBlock > 0) dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);
            bool newTrim = false;

            DateTime timeSpeedStart   = DateTime.UtcNow;
            ulong    sectorSpeedStart = 0;
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

                sense = dev.Read12(out readBuffer, out _, 0, false, true, false, false, (uint)(umdStart + i * 4), 512,
                                   0, blocksToRead * 4, false, dev.Timeout, out double cmdDuration);
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
                    outputPlugin.WriteSectors(new byte[BLOCK_SIZE * skip], i, skip);
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

                currentSpeed     = sectorSpeedStart * BLOCK_SIZE / (1048576 * elapsed);
                sectorSpeedStart = 0;
                timeSpeedStart   = DateTime.UtcNow;
            }

            end = DateTime.UtcNow;
            DicConsole.WriteLine();
            mhddLog.Close();
            ibgLog.Close(dev, blocks, BLOCK_SIZE, (end - start).TotalSeconds, currentSpeed * 1024,
                         BLOCK_SIZE * (double)(blocks + 1) / 1024                          / (totalDuration / 1000),
                         devicePath);
            dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                              (double)BLOCK_SIZE * (double)(blocks + 1) / 1024 / (totalDuration / 1000));
            dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                              (double)BLOCK_SIZE * (double)(blocks + 1) / 1024 / imageWriteDuration);

            #region Trimming
            if(resume.BadBlocks.Count > 0 && !aborted && !notrim && newTrim)
            {
                start = DateTime.UtcNow;
                dumpLog.WriteLine("Trimming bad sectors");

                ulong[] tmpArray = resume.BadBlocks.ToArray();
                foreach(ulong badSector in tmpArray)
                {
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    DicConsole.Write("\rTrimming sector {0}", badSector);

                    sense = dev.Read12(out readBuffer, out _, 0, false, true, false, false,
                                       (uint)(umdStart + badSector * 4), 512, 0, 4, false, dev.Timeout,
                                       out double cmdDuration);

                    if(sense || dev.Error) continue;

                    resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                    outputPlugin.WriteSector(readBuffer, badSector);
                }

                DicConsole.WriteLine();
                end = DateTime.UtcNow;
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

                if(persistent)
                {
                    Modes.ModePage_01 pg;

                    sense = dev.ModeSense6(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                           dev.Timeout, out _);
                    if(!sense)
                    {
                        Modes.DecodedMode? dcMode6 = Modes.DecodeMode6(readBuffer, dev.ScsiType);

                        if(dcMode6.HasValue)
                            foreach(Modes.ModePage modePage in dcMode6.Value.Pages)
                                if(modePage.Page == 0x01 && modePage.Subpage == 0x00)
                                    currentModePage = modePage;
                    }

                    if(currentModePage == null)
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
                    md6 = Modes.EncodeMode6(md, dev.ScsiType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive (return damaged blocks).");
                    sense = dev.ModeSelect(md6, out byte[] senseBuf, true, false, dev.Timeout, out _);

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

                    DicConsole.Write("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass,
                                     forward ? "forward" : "reverse",
                                     runningPersistent ? "recovering partial data, " : "");

                    sense = dev.Read12(out readBuffer, out _, 0, false, true, false, false,
                                       (uint)(umdStart + badSector * 4), 512, 0, 4, false, dev.Timeout,
                                       out double cmdDuration);
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
                        Header = new Modes.ModeHeader(), Pages = new[] {currentModePage.Value}
                    };
                    md6 = Modes.EncodeMode6(md, dev.ScsiType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive (return device to previous status).");
                    sense = dev.ModeSelect(md6, out _, true, false, dev.Timeout, out _);
                }

                DicConsole.WriteLine();
            }
            #endregion Error handling

            resume.BadBlocks.Sort();
            foreach(ulong bad in resume.BadBlocks) dumpLog.WriteLine("Sector {0} could not be read.", bad);
            currentTry.Extents = ExtentsConverter.ToMetadata(extents);

            CommonTypes.Structs.ImageInfo metadata = new CommonTypes.Structs.ImageInfo
            {
                Application        = "DiscImageChef",
                ApplicationVersion = Version.GetVersion(),
                MediaPartNumber    = mediaPartNumber
            };

            if(!outputPlugin.SetMetadata(metadata))
                DicConsole.ErrorWriteLine("Error {0} setting metadata, continuing...", outputPlugin.ErrorMessage);

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
                                  (double)BLOCK_SIZE * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

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
                sidecar.OpticalDisc[0].Dimensions = Dimensions.DimensionsFromMediaType(DSK_TYPE);
                CommonTypes.Metadata.MediaType.MediaTypeToString(DSK_TYPE, out string xmlDskTyp,
                                                                 out string xmlDskSubTyp);
                sidecar.OpticalDisc[0].DiscType          = xmlDskTyp;
                sidecar.OpticalDisc[0].DiscSubType       = xmlDskSubTyp;
                sidecar.OpticalDisc[0].DumpHardwareArray = resume.Tries.ToArray();

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
            DicConsole.WriteLine("Average speed: {0:F3} MiB/sec.",
                                 (double)BLOCK_SIZE * (double)(blocks + 1) / 1048576 / (totalDuration / 1000));
            DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", maxSpeed);
            DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", minSpeed);
            DicConsole.WriteLine("{0} sectors could not be read.",       resume.BadBlocks.Count);
            DicConsole.WriteLine();

            Statistics.AddMedia(DSK_TYPE, true);
        }

        static void DumpMs(Device           dev,          string   devicePath, IWritableImage outputPlugin,
                           ushort           retryPasses,  bool     force,
                           bool             persistent,   bool     stopOnError, ref Resume resume,
                           ref DumpLog      dumpLog,      Encoding encoding,
                           string           outputPrefix, string   outputPath, Dictionary<string, string> formatOptions,
                           CICMMetadataType preSidecar,   uint     skip,       bool                       nometadata,
                           bool             notrim)
        {
            const ushort SBC_PROFILE   = 0x0001;
            const uint   BLOCK_SIZE    = 512;
            double       totalDuration = 0;
            double       currentSpeed  = 0;
            double       maxSpeed      = double.MinValue;
            double       minSpeed      = double.MaxValue;
            bool         aborted       = false;
            uint         blocksToRead  = 64;
            System.Console.CancelKeyPress += (sender, e) => e.Cancel = aborted = true;
            DateTime  start;
            DateTime  end;
            MediaType dskType;
            bool      sense;

            sense = dev.ReadCapacity(out byte[] readBuffer, out _, dev.Timeout, out _);

            if(sense)
            {
                DicConsole.WriteLine("Could not detect capacity...");
                dumpLog.WriteLine("Could not detect capacity...");
                return;
            }

            uint blocks = (uint)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3]);

            blocks++;
            DicConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2} bytes)", blocks,
                                 BLOCK_SIZE, blocks * (ulong)BLOCK_SIZE);

            if(blocks == 0)
            {
                dumpLog.WriteLine("ERROR: Unable to read medium or empty medium present...");
                DicConsole.ErrorWriteLine("Unable to read medium or empty medium present...");
                return;
            }

            dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).",      blocks, blocks * BLOCK_SIZE);
            dumpLog.WriteLine("Device can read {0} blocks at a time.",       blocksToRead);
            dumpLog.WriteLine("Device reports {0} bytes per logical block.", BLOCK_SIZE);
            dumpLog.WriteLine("SCSI device type: {0}.",                      dev.ScsiType);

            if(blocks > 262144)
            {
                dskType = MediaType.MemoryStickProDuo;
                DicConsole.WriteLine("Media detected as MemoryStick Pro Duo...");
                dumpLog.WriteLine("Media detected as MemoryStick Pro Duo...");
            }
            else
            {
                dskType = MediaType.MemoryStickDuo;
                DicConsole.WriteLine("Media detected as MemoryStick Duo...");
                dumpLog.WriteLine("Media detected as MemoryStick Duo...");
            }

            bool ret;

            DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

            MhddLog mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, BLOCK_SIZE, blocksToRead);
            IbgLog  ibgLog  = new IbgLog(outputPrefix  + ".ibg", SBC_PROFILE);
            ret = outputPlugin.Create(outputPath, dskType, formatOptions, blocks, BLOCK_SIZE);

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

            DumpHardwareType currentTry = null;
            ExtentsULong     extents    = null;
            ResumeSupport.Process(true, dev.IsRemovable, blocks, dev.Manufacturer, dev.Model, dev.Serial,
                                  dev.PlatformId, ref resume, ref currentTry, ref extents);
            if(currentTry == null || extents == null)
                throw new InvalidOperationException("Could not process resume file, not continuing...");

            if(resume.NextBlock > 0) dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);
            bool newTrim = false;

            DateTime timeSpeedStart   = DateTime.UtcNow;
            ulong    sectorSpeedStart = 0;
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

                sense = dev.Read12(out readBuffer, out _, 0, false, true, false, false, (uint)i, BLOCK_SIZE, 0,
                                   blocksToRead, false, dev.Timeout, out double cmdDuration);
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
                    outputPlugin.WriteSectors(new byte[BLOCK_SIZE * skip], i, skip);
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

                currentSpeed     = sectorSpeedStart * BLOCK_SIZE / (1048576 * elapsed);
                sectorSpeedStart = 0;
                timeSpeedStart   = DateTime.UtcNow;
            }

            end = DateTime.UtcNow;
            DicConsole.WriteLine();
            mhddLog.Close();
            ibgLog.Close(dev, blocks, BLOCK_SIZE, (end - start).TotalSeconds, currentSpeed * 1024,
                         BLOCK_SIZE * (double)(blocks + 1) / 1024                          / (totalDuration / 1000),
                         devicePath);
            dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                              (double)BLOCK_SIZE * (double)(blocks + 1) / 1024 / (totalDuration / 1000));
            dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                              (double)BLOCK_SIZE * (double)(blocks + 1) / 1024 / imageWriteDuration);

            #region Trimming
            if(resume.BadBlocks.Count > 0 && !aborted && !notrim && newTrim)
            {
                start = DateTime.UtcNow;
                dumpLog.WriteLine("Trimming bad sectors");

                ulong[] tmpArray = resume.BadBlocks.ToArray();
                foreach(ulong badSector in tmpArray)
                {
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    DicConsole.Write("\rTrimming sector {0}", badSector);

                    sense = dev.Read12(out readBuffer, out _, 0, false, true, false, false, (uint)badSector, BLOCK_SIZE,
                                       0, 1, false, dev.Timeout, out double cmdDuration);

                    if(sense || dev.Error) continue;

                    resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                    outputPlugin.WriteSector(readBuffer, badSector);
                }

                DicConsole.WriteLine();
                end = DateTime.UtcNow;
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

                if(persistent)
                {
                    Modes.ModePage_01 pg;

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
                    md6 = Modes.EncodeMode6(md, dev.ScsiType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive (return damaged blocks).");
                    sense = dev.ModeSelect(md6, out byte[] senseBuf, true, false, dev.Timeout, out _);

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

                    DicConsole.Write("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass,
                                     forward ? "forward" : "reverse",
                                     runningPersistent ? "recovering partial data, " : "");

                    sense = dev.Read12(out readBuffer, out _, 0, false, true, false, false, (uint)badSector, BLOCK_SIZE,
                                       0, 1, false, dev.Timeout, out double cmdDuration);
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
                        Header = new Modes.ModeHeader(), Pages = new[] {currentModePage.Value}
                    };
                    md6 = Modes.EncodeMode6(md, dev.ScsiType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive (return device to previous status).");
                    sense = dev.ModeSelect(md6, out _, true, false, dev.Timeout, out _);
                }

                DicConsole.WriteLine();
            }
            #endregion Error handling

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
                                  (double)BLOCK_SIZE * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                if(preSidecar != null)
                {
                    preSidecar.BlockMedia = sidecar.BlockMedia;
                    sidecar               = preSidecar;
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
                CommonTypes.Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp,
                                                                 out string xmlDskSubTyp);
                sidecar.BlockMedia[0].DiskType          = xmlDskTyp;
                sidecar.BlockMedia[0].DiskSubType       = xmlDskSubTyp;
                sidecar.BlockMedia[0].Interface         = "USB";
                sidecar.BlockMedia[0].LogicalBlocks     = blocks;
                sidecar.BlockMedia[0].PhysicalBlockSize = (int)BLOCK_SIZE;
                sidecar.BlockMedia[0].LogicalBlockSize  = (int)BLOCK_SIZE;
                sidecar.BlockMedia[0].Manufacturer      = dev.Manufacturer;
                sidecar.BlockMedia[0].Model             = dev.Model;
                sidecar.BlockMedia[0].Serial            = dev.Serial;
                sidecar.BlockMedia[0].Size              = blocks * BLOCK_SIZE;

                if(dev.IsRemovable) sidecar.BlockMedia[0].DumpHardwareArray = resume.Tries.ToArray();

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
            DicConsole.WriteLine("Average speed: {0:F3} MiB/sec.",
                                 (double)BLOCK_SIZE * (double)(blocks + 1) / 1048576 / (totalDuration / 1000));
            DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", maxSpeed);
            DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", minSpeed);
            DicConsole.WriteLine("{0} sectors could not be read.",       resume.BadBlocks.Count);
            DicConsole.WriteLine();

            Statistics.AddMedia(dskType, true);
        }
    }
}