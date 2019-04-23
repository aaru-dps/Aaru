// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SecureDigital.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps SecureDigital and MultiMediaCard flash cards.
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
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.MMC;
using Schemas;
using MediaType = DiscImageChef.CommonTypes.MediaType;

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>
    ///     Implements dumping a MultiMediaCard or SecureDigital flash card
    /// </summary>
    public partial class Dump
    {
        /// <summary>
        ///     Dumps a MultiMediaCard or SecureDigital flash card
        /// </summary>
        public void SecureDigital()
        {
            if(dumpRaw)
            {
                if(force)
                    ErrorMessage
                      ?.Invoke("Raw dumping is not supported in MultiMediaCard or SecureDigital devices. Continuing...");
                else
                {
                    StoppingErrorMessage
                      ?.Invoke("Raw dumping is not supported in MultiMediaCard or SecureDigital devices. Aborting...");
                    return;
                }
            }

            bool         sense;
            const ushort SD_PROFILE = 0x0001;
            const uint   TIMEOUT    = 5;
            double       duration;

            uint   blocksToRead      = 128;
            uint   blockSize         = 512;
            ulong  blocks            = 0;
            byte[] csd               = null;
            byte[] ocr               = null;
            byte[] ecsd              = null;
            byte[] scr               = null;
            uint   physicalBlockSize = 0;
            bool   byteAddressed     = true;

            Dictionary<MediaTagType, byte[]> mediaTags = new Dictionary<MediaTagType, byte[]>();

            switch(dev.Type)
            {
                case DeviceType.MMC:
                {
                    UpdateStatus?.Invoke("Reading Extended CSD");
                    dumpLog.WriteLine("Reading Extended CSD");
                    sense = dev.ReadExtendedCsd(out ecsd, out _, TIMEOUT, out duration);
                    if(!sense)
                    {
                        ExtendedCSD ecsdDecoded = Decoders.MMC.Decoders.DecodeExtendedCSD(ecsd);
                        blocksToRead = ecsdDecoded.OptimalReadSize;
                        blocks       = ecsdDecoded.SectorCount;
                        blockSize    = (uint)(ecsdDecoded.SectorSize == 1 ? 4096 : 512);
                        if(ecsdDecoded.NativeSectorSize      == 0) physicalBlockSize = 512;
                        else if(ecsdDecoded.NativeSectorSize == 1) physicalBlockSize = 4096;
                        // Supposing it's high-capacity MMC if it has Extended CSD...
                        byteAddressed = false;
                        mediaTags.Add(MediaTagType.MMC_ExtendedCSD, null);
                    }
                    else ecsd = null;

                    UpdateStatus?.Invoke("Reading CSD");
                    dumpLog.WriteLine("Reading CSD");
                    sense = dev.ReadCsd(out csd, out _, TIMEOUT, out duration);
                    if(!sense)
                    {
                        if(blocks == 0)
                        {
                            CSD csdDecoded = Decoders.MMC.Decoders.DecodeCSD(csd);
                            blocks    = (ulong)((csdDecoded.Size + 1) * Math.Pow(2, csdDecoded.SizeMultiplier + 2));
                            blockSize = (uint)Math.Pow(2, csdDecoded.ReadBlockLength);
                        }

                        mediaTags.Add(MediaTagType.MMC_CSD, null);
                    }
                    else csd = null;

                    UpdateStatus?.Invoke("Reading OCR");
                    dumpLog.WriteLine("Reading OCR");
                    sense = dev.ReadOcr(out ocr, out _, TIMEOUT, out duration);
                    if(sense) ocr = null;
                    else mediaTags.Add(MediaTagType.MMC_OCR, null);

                    break;
                }

                case DeviceType.SecureDigital:
                {
                    UpdateStatus?.Invoke("Reading CSD");
                    dumpLog.WriteLine("Reading CSD");
                    sense = dev.ReadCsd(out csd, out _, TIMEOUT, out duration);
                    if(!sense)
                    {
                        Decoders.SecureDigital.CSD csdDecoded = Decoders.SecureDigital.Decoders.DecodeCSD(csd);
                        blocks = (ulong)(csdDecoded.Structure == 0
                                             ? (csdDecoded.Size + 1) * Math.Pow(2, csdDecoded.SizeMultiplier + 2)
                                             : (csdDecoded.Size + 1) * 1024);
                        blockSize = (uint)Math.Pow(2, csdDecoded.ReadBlockLength);
                        // Structure >=1 for SDHC/SDXC, so that's block addressed
                        byteAddressed = csdDecoded.Structure == 0;
                        mediaTags.Add(MediaTagType.SD_CSD, null);
                    }
                    else csd = null;

                    UpdateStatus?.Invoke("Reading OCR");
                    dumpLog.WriteLine("Reading OCR");
                    sense = dev.ReadSdocr(out ocr, out _, TIMEOUT, out duration);
                    if(sense) ocr = null;
                    else mediaTags.Add(MediaTagType.SD_OCR, null);

                    UpdateStatus?.Invoke("Reading SCR");
                    dumpLog.WriteLine("Reading SCR");
                    sense = dev.ReadScr(out scr, out _, TIMEOUT, out duration);
                    if(sense) scr = null;
                    else mediaTags.Add(MediaTagType.SD_SCR, null);

                    break;
                }
            }

            UpdateStatus?.Invoke("Reading CID");
            dumpLog.WriteLine("Reading CID");
            sense = dev.ReadCid(out byte[] cid, out _, TIMEOUT, out duration);
            if(sense) cid = null;
            else mediaTags.Add(dev.Type == DeviceType.SecureDigital ? MediaTagType.SD_CID : MediaTagType.MMC_CID, null);

            DateTime start;
            DateTime end;
            double   totalDuration = 0;
            double   currentSpeed  = 0;
            double   maxSpeed      = double.MinValue;
            double   minSpeed      = double.MaxValue;

            if(blocks == 0)
            {
                dumpLog.WriteLine("Unable to get device size.");
                StoppingErrorMessage?.Invoke("Unable to get device size.");
                return;
            }

            UpdateStatus?.Invoke($"Device reports {blocks} blocks.");
            dumpLog.WriteLine("Device reports {0} blocks.", blocks);

            byte[] cmdBuf;
            bool   error;

            while(true)
            {
                error = dev.Read(out cmdBuf, out _, 0, blockSize, blocksToRead, byteAddressed, TIMEOUT, out duration);

                if(error) blocksToRead /= 2;

                if(!error || blocksToRead == 1) break;
            }

            if(error)
            {
                dumpLog.WriteLine("ERROR: Cannot get blocks to read, device error {0}.", dev.LastError);
                StoppingErrorMessage?.Invoke($"Device error {dev.LastError} trying to guess ideal transfer length.");
                return;
            }

            UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time.");
            dumpLog.WriteLine("Device can read {0} blocks at a time.", blocksToRead);

            if(skip < blocksToRead) skip = blocksToRead;

            DumpHardwareType currentTry = null;
            ExtentsULong     extents    = null;
            ResumeSupport.Process(true, false, blocks, dev.Manufacturer, dev.Model, dev.Serial, dev.PlatformId,
                                  ref resume, ref currentTry, ref extents);
            if(currentTry == null || extents == null)
            {
                StoppingErrorMessage?.Invoke("Could not process resume file, not continuing...");
                return;
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

            MhddLog mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
            IbgLog  ibgLog  = new IbgLog(outputPrefix  + ".ibg", SD_PROFILE);
            ret = outputPlugin.Create(outputPath,
                                      dev.Type == DeviceType.SecureDigital ? MediaType.SecureDigital : MediaType.MMC,
                                      formatOptions, blocks, blockSize);

            // Cannot create image
            if(!ret)
            {
                dumpLog.WriteLine("Error creating output image, not continuing.");
                dumpLog.WriteLine(outputPlugin.ErrorMessage);
                StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                             outputPlugin.ErrorMessage);
                return;
            }

            if(resume.NextBlock > 0)
            {
                UpdateStatus?.Invoke($"Resuming from block {resume.NextBlock}.");
                dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);
            }

            start = DateTime.UtcNow;
            double   imageWriteDuration = 0;
            bool     newTrim            = false;
            DateTime timeSpeedStart     = DateTime.UtcNow;
            ulong    sectorSpeedStart   = 0;

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

                if(blocks - i < blocksToRead) blocksToRead = (byte)(blocks - i);

                #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                UpdateProgress?.Invoke($"Reading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)", (long)i,
                                       (long)blocks);

                error = dev.Read(out cmdBuf, out _, (uint)i, blockSize, blocksToRead, byteAddressed, TIMEOUT,
                                 out duration);

                if(!error)
                {
                    mhddLog.Write(i, duration);
                    ibgLog.Write(i, currentSpeed * 1024);
                    DateTime writeStart = DateTime.Now;
                    outputPlugin.WriteSectors(cmdBuf, i, blocksToRead);
                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                    extents.Add(i, blocksToRead, true);
                }
                else
                {
                    if(i + skip > blocks) skip = (uint)(blocks - i);

                    for(ulong b = i; b < i + skip; b++) resume.BadBlocks.Add(b);

                    mhddLog.Write(i, duration < 500 ? 65535 : duration);

                    ibgLog.Write(i, 0);
                    DateTime writeStart = DateTime.Now;
                    outputPlugin.WriteSectors(new byte[blockSize * skip], i, skip);
                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
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

            end = DateTime.Now;
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

                    error = dev.Read(out cmdBuf, out _, (uint)badSector, blockSize, 1, byteAddressed, TIMEOUT,
                                     out duration);

                    totalDuration += duration;

                    if(error) continue;

                    resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                    outputPlugin.WriteSector(cmdBuf, badSector);
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

                InitProgress?.Invoke();
                repeatRetryLba:
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

                    error = dev.Read(out cmdBuf, out _, (uint)badSector, blockSize, 1, byteAddressed, TIMEOUT,
                                     out duration);

                    totalDuration += duration;

                    if(!error)
                    {
                        resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        outputPlugin.WriteSector(cmdBuf, badSector);
                        UpdateStatus?.Invoke($"Correctly retried block {badSector} in pass {pass}.");
                        dumpLog.WriteLine("Correctly retried block {0} in pass {1}.", badSector, pass);
                    }
                    else if(runningPersistent) outputPlugin.WriteSector(cmdBuf, badSector);
                }

                if(pass < retryPasses && !aborted && resume.BadBlocks.Count > 0)
                {
                    pass++;
                    forward = !forward;
                    resume.BadBlocks.Sort();
                    resume.BadBlocks.Reverse();
                    goto repeatRetryLba;
                }

                EndProgress?.Invoke();
            }
            #endregion Error handling

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
                if(!inputPlugin.Open(filter)) StoppingErrorMessage?.Invoke("Could not open created image.");

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

                if(preSidecar != null)
                {
                    preSidecar.BlockMedia = sidecar.BlockMedia;
                    sidecar               = preSidecar;
                }

                switch(dev.Type)
                {
                    case DeviceType.MMC:
                        sidecar.BlockMedia[0].MultiMediaCard = new MultiMediaCardType();
                        break;
                    case DeviceType.SecureDigital:
                        sidecar.BlockMedia[0].SecureDigital = new SecureDigitalType();
                        break;
                }

                DumpType cidDump = null;
                DumpType csdDump = null;
                DumpType ocrDump = null;

                if(cid != null)
                {
                    cidDump = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)cid.Length,
                        Checksums = Checksum.GetChecksums(cid).ToArray()
                    };

                    ret =
                        outputPlugin.WriteMediaTag(cid,
                                                   dev.Type == DeviceType.SecureDigital
                                                       ? MediaTagType.SD_CID
                                                       : MediaTagType.MMC_CID);

                    // Cannot write CID to image
                    if(!ret && !force)
                    {
                        dumpLog.WriteLine("Cannot write CID to output image.");
                        StoppingErrorMessage?.Invoke("Cannot write CID to output image." + Environment.NewLine +
                                                     outputPlugin.ErrorMessage);
                        return;
                    }
                }

                if(csd != null)
                {
                    csdDump = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)csd.Length,
                        Checksums = Checksum.GetChecksums(csd).ToArray()
                    };

                    ret =
                        outputPlugin.WriteMediaTag(csd,
                                                   dev.Type == DeviceType.SecureDigital
                                                       ? MediaTagType.SD_CSD
                                                       : MediaTagType.MMC_CSD);

                    // Cannot write CSD to image
                    if(!ret && !force)
                    {
                        dumpLog.WriteLine("Cannot write CSD to output image.");
                        StoppingErrorMessage?.Invoke("Cannot write CSD to output image." + Environment.NewLine +
                                                     outputPlugin.ErrorMessage);
                        return;
                    }
                }

                if(ecsd != null)
                {
                    sidecar.BlockMedia[0].MultiMediaCard.ExtendedCSD = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)ecsd.Length,
                        Checksums = Checksum.GetChecksums(ecsd).ToArray()
                    };

                    ret = outputPlugin.WriteMediaTag(ecsd, MediaTagType.MMC_ExtendedCSD);

                    // Cannot write Extended CSD to image
                    if(!ret && !force)
                    {
                        dumpLog.WriteLine("Cannot write Extended CSD to output image.");
                        StoppingErrorMessage?.Invoke("Cannot write Extended CSD to output image." +
                                                     Environment.NewLine                          +
                                                     outputPlugin.ErrorMessage);
                        return;
                    }
                }

                if(ocr != null)
                {
                    ocrDump = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)ocr.Length,
                        Checksums = Checksum.GetChecksums(ocr).ToArray()
                    };

                    ret =
                        outputPlugin.WriteMediaTag(ocr,
                                                   dev.Type == DeviceType.SecureDigital
                                                       ? MediaTagType.SD_OCR
                                                       : MediaTagType.MMC_OCR);

                    // Cannot write OCR to image
                    if(!ret && !force)
                    {
                        dumpLog.WriteLine("Cannot write OCR to output image.");
                        StoppingErrorMessage?.Invoke("Cannot write OCR to output image." + Environment.NewLine +
                                                     outputPlugin.ErrorMessage);
                        return;
                    }
                }

                if(scr != null)
                {
                    sidecar.BlockMedia[0].SecureDigital.SCR = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)scr.Length,
                        Checksums = Checksum.GetChecksums(scr).ToArray()
                    };

                    ret = outputPlugin.WriteMediaTag(scr, MediaTagType.SD_SCR);

                    // Cannot write SCR to image
                    if(!ret && !force)
                    {
                        dumpLog.WriteLine("Cannot write SCR to output image.");
                        StoppingErrorMessage?.Invoke("Cannot write SCR to output image." + Environment.NewLine +
                                                     outputPlugin.ErrorMessage);
                        return;
                    }
                }

                switch(dev.Type)
                {
                    case DeviceType.MMC:
                        sidecar.BlockMedia[0].MultiMediaCard.CID = cidDump;
                        sidecar.BlockMedia[0].MultiMediaCard.CSD = csdDump;
                        sidecar.BlockMedia[0].MultiMediaCard.OCR = ocrDump;
                        break;
                    case DeviceType.SecureDigital:
                        sidecar.BlockMedia[0].SecureDigital.CID = cidDump;
                        sidecar.BlockMedia[0].SecureDigital.CSD = csdDump;
                        sidecar.BlockMedia[0].SecureDigital.OCR = ocrDump;
                        break;
                }

                end = DateTime.UtcNow;

                totalChkDuration = (end - chkStart).TotalMilliseconds;
                UpdateStatus?.Invoke($"Sidecar created in {(end - chkStart).TotalSeconds} seconds.");
                UpdateStatus
                  ?.Invoke($"Average checksum speed {(double)blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000):F3} KiB/sec.");
                dumpLog.WriteLine("Sidecar created in {0} seconds.", (end - chkStart).TotalSeconds);
                dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                                  (double)blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                string xmlDskTyp = null, xmlDskSubTyp = null;
                switch(dev.Type)
                {
                    case DeviceType.MMC:
                        CommonTypes.Metadata.MediaType.MediaTypeToString(MediaType.MMC, out xmlDskTyp,
                                                                         out xmlDskSubTyp);
                        sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(MediaType.MMC);
                        break;
                    case DeviceType.SecureDigital:
                        CommonTypes.Metadata.MediaType.MediaTypeToString(MediaType.SecureDigital, out xmlDskTyp,
                                                                         out xmlDskSubTyp);
                        sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(MediaType.SecureDigital);
                        break;
                }

                sidecar.BlockMedia[0].DiskType    = xmlDskTyp;
                sidecar.BlockMedia[0].DiskSubType = xmlDskSubTyp;
                // TODO: Implement device firmware revision
                sidecar.BlockMedia[0].LogicalBlocks     = blocks;
                sidecar.BlockMedia[0].PhysicalBlockSize = physicalBlockSize > 0 ? physicalBlockSize : blockSize;
                sidecar.BlockMedia[0].LogicalBlockSize  = blockSize;
                sidecar.BlockMedia[0].Manufacturer      = dev.Manufacturer;
                sidecar.BlockMedia[0].Model             = dev.Model;
                sidecar.BlockMedia[0].Serial            = dev.Serial;
                sidecar.BlockMedia[0].Size              = blocks * blockSize;

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
            if(resume.BadBlocks.Count > 0) resume.BadBlocks.Sort();

            switch(dev.Type)
            {
                case DeviceType.MMC:
                    Statistics.AddMedia(MediaType.MMC, true);
                    break;
                case DeviceType.SecureDigital:
                    Statistics.AddMedia(MediaType.SecureDigital, true);
                    break;
            }
        }
    }
}