// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ATA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps media from ATA devices.
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
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.PCMCIA;
using Schemas;
using Tuple = DiscImageChef.Decoders.PCMCIA.Tuple;

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>
    ///     Implements dumping ATA devices
    /// </summary>
    public partial class Dump
    {
        /// <summary>
        ///     Dumps an ATA device
        /// </summary>
        public void Ata()
        {
            if(dumpRaw)
            {
                if(force) ErrorMessage?.Invoke("Raw dumping not yet supported in ATA devices, continuing...");
                else
                {
                    StoppingErrorMessage?.Invoke("Raw dumping not yet supported in ATA devices, aborting...");
                    return;
                }
            }

            const ushort ATA_PROFILE        = 0x0001;
            const uint   TIMEOUT            = 5;
            double       imageWriteDuration = 0;

            UpdateStatus?.Invoke("Requesting ATA IDENTIFY DEVICE.");
            dumpLog.WriteLine("Requesting ATA IDENTIFY DEVICE.");
            bool sense = dev.AtaIdentify(out byte[] cmdBuf, out _);
            if(!sense && Identify.Decode(cmdBuf).HasValue)
            {
                Identify.IdentifyDevice? ataIdNullable = Identify.Decode(cmdBuf);
                if(ataIdNullable != null)
                {
                    Identify.IdentifyDevice ataId       = ataIdNullable.Value;
                    byte[]                  ataIdentify = cmdBuf;
                    cmdBuf = new byte[0];

                    DateTime start;
                    DateTime end;
                    double   totalDuration = 0;
                    double   currentSpeed  = 0;
                    double   maxSpeed      = double.MinValue;
                    double   minSpeed      = double.MaxValue;

                    // Initializate reader
                    UpdateStatus?.Invoke("Initializing reader.");
                    dumpLog.WriteLine("Initializing reader.");
                    Reader ataReader = new Reader(dev, TIMEOUT, ataIdentify);
                    // Fill reader blocks
                    ulong blocks = ataReader.GetDeviceBlocks();
                    // Check block sizes
                    if(ataReader.GetBlockSize())
                    {
                        dumpLog.WriteLine("ERROR: Cannot get block size: {0}.", ataReader.ErrorMessage);
                        ErrorMessage(ataReader.ErrorMessage);
                        return;
                    }

                    uint blockSize          = ataReader.LogicalBlockSize;
                    uint physicalsectorsize = ataReader.PhysicalBlockSize;
                    if(ataReader.FindReadCommand())
                    {
                        dumpLog.WriteLine("ERROR: Cannot find correct read command: {0}.", ataReader.ErrorMessage);
                        ErrorMessage(ataReader.ErrorMessage);
                        return;
                    }

                    // Check how many blocks to read, if error show and return
                    if(ataReader.GetBlocksToRead())
                    {
                        dumpLog.WriteLine("ERROR: Cannot get blocks to read: {0}.", ataReader.ErrorMessage);
                        ErrorMessage(ataReader.ErrorMessage);
                        return;
                    }

                    uint   blocksToRead = ataReader.BlocksToRead;
                    ushort cylinders    = ataReader.Cylinders;
                    byte   heads        = ataReader.Heads;
                    byte   sectors      = ataReader.Sectors;

                    UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");
                    UpdateStatus
                      ?.Invoke($"Device reports {cylinders} cylinders {heads} heads {sectors} sectors per track.");
                    UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time.");
                    UpdateStatus?.Invoke($"Device reports {blockSize} bytes per logical block.");
                    UpdateStatus?.Invoke($"Device reports {physicalsectorsize} bytes per physical block.");
                    dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).", blocks, blocks * blockSize);
                    dumpLog.WriteLine("Device reports {0} cylinders {1} heads {2} sectors per track.", cylinders, heads,
                                      sectors);
                    dumpLog.WriteLine("Device can read {0} blocks at a time.",        blocksToRead);
                    dumpLog.WriteLine("Device reports {0} bytes per logical block.",  blockSize);
                    dumpLog.WriteLine("Device reports {0} bytes per physical block.", physicalsectorsize);

                    bool removable = !dev.IsCompactFlash &&
                                     ataId.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.Removable);
                    DumpHardwareType currentTry = null;
                    ExtentsULong     extents    = null;
                    ResumeSupport.Process(ataReader.IsLba, removable, blocks, dev.Manufacturer, dev.Model, dev.Serial,
                                          dev.PlatformId, ref resume, ref currentTry, ref extents);
                    if(currentTry == null || extents == null)
                    {
                        StoppingErrorMessage?.Invoke("Could not process resume file, not continuing...");
                        return;
                    }

                    MhddLog mhddLog;
                    IbgLog  ibgLog;
                    double  duration;

                    bool ret = true;

                    if(dev.IsUsb && dev.UsbDescriptors != null &&
                       !outputPlugin.SupportedMediaTags.Contains(MediaTagType.USB_Descriptors))
                    {
                        ret = false;
                        dumpLog.WriteLine("Output format does not support USB descriptors.");
                        ErrorMessage("Output format does not support USB descriptors.");
                    }

                    if(dev.IsPcmcia && dev.Cis != null &&
                       !outputPlugin.SupportedMediaTags.Contains(MediaTagType.PCMCIA_CIS))
                    {
                        ret = false;
                        dumpLog.WriteLine("Output format does not support PCMCIA CIS descriptors.");
                        ErrorMessage("Output format does not support PCMCIA CIS descriptors.");
                    }

                    if(!outputPlugin.SupportedMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
                    {
                        ret = false;
                        dumpLog.WriteLine("Output format does not support ATA IDENTIFY.");
                        ErrorMessage("Output format does not support ATA IDENTIFY.");
                    }

                    if(!ret)
                    {
                        dumpLog.WriteLine("Several media tags not supported, {0}continuing...", force ? "" : "not ");
                        if(force) ErrorMessage("Several media tags not supported, continuing...");
                        else
                        {
                            StoppingErrorMessage?.Invoke("Several media tags not supported, not continuing...");
                            return;
                        }
                    }

                    ret = outputPlugin.Create(outputPath,
                                              dev.IsCompactFlash ? MediaType.CompactFlash : MediaType.GENERIC_HDD,
                                              formatOptions, blocks, blockSize);

                    // Cannot create image
                    if(!ret)
                    {
                        dumpLog.WriteLine("Error creating output image, not continuing.");
                        dumpLog.WriteLine(outputPlugin.ErrorMessage);
                        StoppingErrorMessage?.Invoke("Error creating output image, not continuing." +
                                                     Environment.NewLine                            +
                                                     outputPlugin.ErrorMessage);
                        return;
                    }

                    // Setting geometry
                    outputPlugin.SetGeometry(cylinders, heads, sectors);

                    if(ataReader.IsLba)
                    {
                        UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time.");
                        if(skip < blocksToRead) skip = blocksToRead;

                        mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
                        ibgLog  = new IbgLog(outputPrefix  + ".ibg", ATA_PROFILE);

                        if(resume.NextBlock > 0)
                        {
                            UpdateStatus?.Invoke($"Resuming from block {resume.NextBlock}.");
                            dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);
                        }

                        bool newTrim = false;

                        start = DateTime.UtcNow;
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

                            if(blocks - i < blocksToRead) blocksToRead = (byte)(blocks - i);

                            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                            if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                            #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            UpdateProgress?.Invoke($"\rReading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)",
                                                   (long)i, (long)blocks);

                            bool error = ataReader.ReadBlocks(out cmdBuf, i, blocksToRead, out duration);

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
                                     blockSize * (double)(blocks + 1) / 1024 /
                                     (totalDuration / 1000), devicePath);

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

                                PulseProgress?.Invoke($"\rTrimming sector {badSector}");

                                bool error = ataReader.ReadBlock(out cmdBuf, badSector, out duration);

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
                            int  pass    = 1;
                            bool forward = true;

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

                                PulseProgress?.Invoke(string.Format("\rRetrying sector {0}, pass {1}, {3}{2}",
                                                                    badSector, pass, forward ? "forward" : "reverse",
                                                                    persistent ? "recovering partial data, " : ""));

                                bool error = ataReader.ReadBlock(out cmdBuf, badSector, out duration);

                                totalDuration += duration;

                                if(!error)
                                {
                                    resume.BadBlocks.Remove(badSector);
                                    extents.Add(badSector);
                                    outputPlugin.WriteSector(cmdBuf, badSector);
                                    UpdateStatus?.Invoke($"Correctly retried block {badSector} in pass {pass}.");
                                    dumpLog.WriteLine("Correctly retried block {0} in pass {1}.", badSector, pass);
                                }
                                else if(persistent) outputPlugin.WriteSector(cmdBuf, badSector);
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
                        #endregion Error handling LBA

                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    }
                    else
                    {
                        mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
                        ibgLog  = new IbgLog(outputPrefix  + ".ibg", ATA_PROFILE);

                        ulong currentBlock = 0;
                        blocks = (ulong)(cylinders * heads * sectors);
                        start  = DateTime.UtcNow;
                        DateTime timeSpeedStart   = DateTime.UtcNow;
                        ulong    sectorSpeedStart = 0;
                        InitProgress?.Invoke();
                        for(ushort cy = 0; cy < cylinders; cy++)
                        {
                            for(byte hd = 0; hd < heads; hd++)
                            {
                                for(byte sc = 1; sc < sectors; sc++)
                                {
                                    if(aborted)
                                    {
                                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                        UpdateStatus?.Invoke("Aborted!");
                                        dumpLog.WriteLine("Aborted!");
                                        break;
                                    }

                                    #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                                    if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                                    if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                                    #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                                    PulseProgress
                                      ?.Invoke($"\rReading cylinder {cy} head {hd} sector {sc} ({currentSpeed:F3} MiB/sec.)");

                                    bool error = ataReader.ReadChs(out cmdBuf, cy, hd, sc, out duration);

                                    totalDuration += duration;

                                    if(!error)
                                    {
                                        mhddLog.Write(currentBlock, duration);
                                        ibgLog.Write(currentBlock, currentSpeed * 1024);
                                        DateTime writeStart = DateTime.Now;
                                        outputPlugin.WriteSector(cmdBuf,
                                                                 (ulong)((cy * heads + hd) * sectors + (sc - 1)));
                                        imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                                        extents.Add(currentBlock);
                                        dumpLog.WriteLine("Error reading cylinder {0} head {1} sector {2}.", cy, hd,
                                                          sc);
                                    }
                                    else
                                    {
                                        resume.BadBlocks.Add(currentBlock);
                                        mhddLog.Write(currentBlock, duration < 500 ? 65535 : duration);

                                        ibgLog.Write(currentBlock, 0);
                                        DateTime writeStart = DateTime.Now;
                                        outputPlugin.WriteSector(new byte[blockSize],
                                                                 (ulong)((cy * heads + hd) * sectors + (sc - 1)));
                                        imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                                    }

                                    sectorSpeedStart++;
                                    currentBlock++;

                                    double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;
                                    if(elapsed < 1) continue;

                                    currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                                    sectorSpeedStart = 0;
                                    timeSpeedStart   = DateTime.UtcNow;
                                }
                            }
                        }

                        end = DateTime.Now;
                        EndProgress?.Invoke();
                        mhddLog.Close();
                        ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                     blockSize * (double)(blocks + 1) / 1024 /
                                     (totalDuration / 1000), devicePath);
                        UpdateStatus?.Invoke($"Dump finished in {(end - start).TotalSeconds} seconds.");
                        UpdateStatus
                          ?.Invoke($"Average dump speed {(double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000):F3} KiB/sec.");
                        UpdateStatus
                          ?.Invoke($"Average write speed {(double)blockSize * (double)(blocks + 1) / 1024 / (imageWriteDuration / 1000):F3} KiB/sec.");
                        dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
                        dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                                          (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));
                        dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                                          (double)blockSize * (double)(blocks + 1) / 1024 /
                                          (imageWriteDuration / 1000));
                    }

                    foreach(ulong bad in resume.BadBlocks) dumpLog.WriteLine("Sector {0} could not be read.", bad);
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
                        dumpLog.WriteLine("Aborted!");
                        UpdateStatus?.Invoke("Aborted!");
                        return;
                    }

                    double totalChkDuration = 0;
                    if(!nometadata)
                    {
                        dumpLog.WriteLine("Creating sidecar.");
                        UpdateStatus?.Invoke("Creating sidecar.");
                        FiltersList filters     = new FiltersList();
                        IFilter     filter      = filters.GetFilter(outputPath);
                        IMediaImage inputPlugin = ImageFormat.Detect(filter);
                        if(!inputPlugin.Open(filter))
                        {
                            StoppingErrorMessage?.Invoke("Could not open created image.");
                            return;
                        }

                        DateTime chkStart = DateTime.UtcNow;
                        Sidecar sidecarClass = new Sidecar(inputPlugin, outputPath, filter.Id, encoding)
                        {
                            // TODO: Be able to cancel hashing
                            InitProgressEvent += InitProgress,
                            UpdateProgressEvent += UpdateProgress,
                            EndProgressEvent += EndProgress,
                            InitProgressEvent2 += InitProgress2,
                            UpdateProgressEvent2 += UpdateProgress2,
                            EndProgressEvent2 += EndProgress2,
                            UpdateStatusEvent += UpdateStatus
                        };
                        CICMMetadataType sidecar = sidecarClass.Create();
                        if(preSidecar != null)
                        {
                            preSidecar.BlockMedia = sidecar.BlockMedia;
                            sidecar               = preSidecar;
                        }

                        if(dev.IsUsb)
                        {
                            dumpLog.WriteLine("Reading USB descriptors.");
                            UpdateStatus?.Invoke("Reading USB descriptors.");
                            ret = outputPlugin.WriteMediaTag(dev.UsbDescriptors, MediaTagType.USB_Descriptors);

                            if(ret)
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
                        }

                        if(dev.IsPcmcia)
                        {
                            dumpLog.WriteLine("Reading PCMCIA CIS.");
                            UpdateStatus?.Invoke("Reading PCMCIA CIS.");
                            ret = outputPlugin.WriteMediaTag(dev.Cis, MediaTagType.PCMCIA_CIS);

                            if(ret)
                                sidecar.BlockMedia[0].PCMCIA = new PCMCIAType
                                {
                                    CIS = new DumpType
                                    {
                                        Image     = outputPath,
                                        Size      = dev.Cis.Length,
                                        Checksums = Checksum.GetChecksums(dev.Cis).ToArray()
                                    }
                                };

                            dumpLog.WriteLine("Decoding PCMCIA CIS.");
                            UpdateStatus?.Invoke("Decoding PCMCIA CIS.");
                            Tuple[] tuples = CIS.GetTuples(dev.Cis);
                            if(tuples != null)
                                foreach(Tuple tuple in tuples)
                                    switch(tuple.Code)
                                    {
                                        case TupleCodes.CISTPL_MANFID:
                                            ManufacturerIdentificationTuple manfid =
                                                CIS.DecodeManufacturerIdentificationTuple(tuple);

                                            if(manfid != null)
                                            {
                                                sidecar.BlockMedia[0].PCMCIA.ManufacturerCode =
                                                    manfid.ManufacturerID;
                                                sidecar.BlockMedia[0].PCMCIA.CardCode                  = manfid.CardID;
                                                sidecar.BlockMedia[0].PCMCIA.ManufacturerCodeSpecified = true;
                                                sidecar.BlockMedia[0].PCMCIA.CardCodeSpecified         = true;
                                            }

                                            break;
                                        case TupleCodes.CISTPL_VERS_1:
                                            Level1VersionTuple vers = CIS.DecodeLevel1VersionTuple(tuple);

                                            if(vers != null)
                                            {
                                                sidecar.BlockMedia[0].PCMCIA.Manufacturer = vers.Manufacturer;
                                                sidecar.BlockMedia[0].PCMCIA.ProductName  = vers.Product;
                                                sidecar.BlockMedia[0].PCMCIA.Compliance =
                                                    $"{vers.MajorVersion}.{vers.MinorVersion}";
                                                sidecar.BlockMedia[0].PCMCIA.AdditionalInformation =
                                                    vers.AdditionalInformation;
                                            }

                                            break;
                                    }
                        }

                        ret = outputPlugin.WriteMediaTag(ataIdentify, MediaTagType.ATA_IDENTIFY);

                        if(ret)
                            sidecar.BlockMedia[0].ATA = new ATAType
                            {
                                Identify = new DumpType
                                {
                                    Image     = outputPath,
                                    Size      = cmdBuf.Length,
                                    Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                }
                            };

                        DateTime chkEnd = DateTime.UtcNow;

                        totalChkDuration = (chkEnd - chkStart).TotalMilliseconds;
                        UpdateStatus?.Invoke($"Sidecar created in {(chkEnd - chkStart).TotalSeconds} seconds.");
                        UpdateStatus
                          ?.Invoke($"Average checksum speed {(double)blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000):F3} KiB/sec.");
                        dumpLog.WriteLine("Sidecar created in {0} seconds.", (chkEnd - chkStart).TotalSeconds);
                        dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                                          (double)blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                        List<(ulong start, string type)> filesystems = new List<(ulong start, string type)>();
                        if(sidecar.BlockMedia[0].FileSystemInformation != null)
                            filesystems.AddRange(from partition in sidecar.BlockMedia[0].FileSystemInformation
                                                 where partition.FileSystems != null
                                                 from fileSystem in partition.FileSystems
                                                 select ((ulong)partition.StartSector, fileSystem.Type));

                        if(filesystems.Count > 0)
                            foreach(var filesystem in filesystems.Select(o => new {o.start, o.type}).Distinct())
                            {
                                UpdateStatus
                                  ?.Invoke($"Found filesystem {filesystem.type} at sector {filesystem.start}");
                                dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type,
                                                  filesystem.start);
                            }

                        string xmlDskTyp, xmlDskSubTyp;
                        if(dev.IsCompactFlash)
                            CommonTypes.Metadata.MediaType.MediaTypeToString(MediaType.CompactFlash, out xmlDskTyp,
                                                                             out xmlDskSubTyp);
                        else if(dev.IsPcmcia)
                            CommonTypes.Metadata.MediaType.MediaTypeToString(MediaType.PCCardTypeI, out xmlDskTyp,
                                                                             out xmlDskSubTyp);
                        else
                            CommonTypes.Metadata.MediaType.MediaTypeToString(MediaType.GENERIC_HDD, out xmlDskTyp,
                                                                             out xmlDskSubTyp);
                        sidecar.BlockMedia[0].DiskType          = xmlDskTyp;
                        sidecar.BlockMedia[0].DiskSubType       = xmlDskSubTyp;
                        sidecar.BlockMedia[0].Interface         = "ATA";
                        sidecar.BlockMedia[0].LogicalBlocks     = (long)blocks;
                        sidecar.BlockMedia[0].PhysicalBlockSize = (int)physicalsectorsize;
                        sidecar.BlockMedia[0].LogicalBlockSize  = (int)blockSize;
                        sidecar.BlockMedia[0].Manufacturer      = dev.Manufacturer;
                        sidecar.BlockMedia[0].Model             = dev.Model;
                        sidecar.BlockMedia[0].Serial            = dev.Serial;
                        sidecar.BlockMedia[0].Size              = (long)(blocks * blockSize);
                        if(cylinders > 0 && heads > 0 && sectors > 0)
                        {
                            sidecar.BlockMedia[0].Cylinders                = cylinders;
                            sidecar.BlockMedia[0].CylindersSpecified       = true;
                            sidecar.BlockMedia[0].Heads                    = heads;
                            sidecar.BlockMedia[0].HeadsSpecified           = true;
                            sidecar.BlockMedia[0].SectorsPerTrack          = sectors;
                            sidecar.BlockMedia[0].SectorsPerTrackSpecified = true;
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
                    if(resume.BadBlocks.Count > 0) resume.BadBlocks.Sort();
                    UpdateStatus?.Invoke("");
                }

                if(dev.IsCompactFlash) Statistics.AddMedia(MediaType.CompactFlash, true);
                else if(dev.IsPcmcia) Statistics.AddMedia(MediaType.PCCardTypeI,   true);
                else Statistics.AddMedia(MediaType.GENERIC_HDD,                    true);
            }
            else StoppingErrorMessage?.Invoke("Unable to communicate with ATA device.");
        }
    }
}