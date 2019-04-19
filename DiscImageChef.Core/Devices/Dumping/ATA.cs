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
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Extents;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Devices;
using Schemas;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using Tuple = DiscImageChef.Decoders.PCMCIA.Tuple;

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>
    ///     Implements dumping ATA devices
    /// </summary>
    public static class Ata
    {
        /// <summary>
        ///     Dumps an ATA device
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="outputPrefix">Prefix for output log files</param>
        /// <param name="outputPlugin">Plugin for output file</param>
        /// <param name="retryPasses">How many times to retry</param>
        /// <param name="force">Force to continue dump whenever possible</param>
        /// <param name="dumpRaw">Dump long sectors</param>
        /// <param name="persistent">Store whatever data the drive returned on error</param>
        /// <param name="stopOnError">Stop dump on first error</param>
        /// <param name="resume">Information for dump resuming</param>
        /// <param name="dumpLog">Dump logger</param>
        /// <param name="encoding">Encoding to use when analyzing dump</param>
        /// <param name="outputPath">Path to output file</param>
        /// <param name="formatOptions">Formats to pass to output file plugin</param>
        /// <exception cref="InvalidOperationException">If the resume file is invalid</exception>
        public static void Dump(Device                     dev,           string           devicePath,
                                IWritableImage             outputPlugin,  ushort           retryPasses,
                                bool                       force,         bool             dumpRaw,
                                bool                       persistent,    bool             stopOnError, ref Resume resume,
                                ref DumpLog                dumpLog,       Encoding         encoding,
                                string                     outputPrefix,  string           outputPath,
                                Dictionary<string, string> formatOptions, CICMMetadataType preSidecar,
                                uint                       skip,
                                bool                       nometadata, bool notrim)
        {
            bool aborted;

            if(dumpRaw)
            {
                DicConsole.ErrorWriteLine("Raw dumping not yet supported in ATA devices.");

                if(force) DicConsole.ErrorWriteLine("Continuing...");
                else
                {
                    DicConsole.ErrorWriteLine("Aborting...");
                    return;
                }
            }

            bool         sense;
            const ushort ATA_PROFILE        = 0x0001;
            const uint   TIMEOUT            = 5;
            double       imageWriteDuration = 0;

            dumpLog.WriteLine("Requesting ATA IDENTIFY DEVICE.");
            sense = dev.AtaIdentify(out byte[] cmdBuf, out _);
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

                    aborted                       =  false;
                    System.Console.CancelKeyPress += (sender, e) => e.Cancel = aborted = true;

                    // Initializate reader
                    dumpLog.WriteLine("Initializing reader.");
                    Reader ataReader = new Reader(dev, TIMEOUT, ataIdentify);
                    // Fill reader blocks
                    ulong blocks = ataReader.GetDeviceBlocks();
                    // Check block sizes
                    if(ataReader.GetBlockSize())
                    {
                        dumpLog.WriteLine("ERROR: Cannot get block size: {0}.", ataReader.ErrorMessage);
                        DicConsole.ErrorWriteLine(ataReader.ErrorMessage);
                        return;
                    }

                    uint blockSize          = ataReader.LogicalBlockSize;
                    uint physicalsectorsize = ataReader.PhysicalBlockSize;
                    if(ataReader.FindReadCommand())
                    {
                        dumpLog.WriteLine("ERROR: Cannot find correct read command: {0}.", ataReader.ErrorMessage);
                        DicConsole.ErrorWriteLine(ataReader.ErrorMessage);
                        return;
                    }

                    // Check how many blocks to read, if error show and return
                    if(ataReader.GetBlocksToRead())
                    {
                        dumpLog.WriteLine("ERROR: Cannot get blocks to read: {0}.", ataReader.ErrorMessage);
                        DicConsole.ErrorWriteLine(ataReader.ErrorMessage);
                        return;
                    }

                    uint   blocksToRead = ataReader.BlocksToRead;
                    ushort cylinders    = ataReader.Cylinders;
                    byte   heads        = ataReader.Heads;
                    byte   sectors      = ataReader.Sectors;

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
                        throw new InvalidOperationException("Could not process resume file, not continuing...");

                    MhddLog mhddLog;
                    IbgLog  ibgLog;
                    double  duration;

                    bool ret = true;

                    if(dev.IsUsb && dev.UsbDescriptors != null &&
                       !outputPlugin.SupportedMediaTags.Contains(MediaTagType.USB_Descriptors))
                    {
                        ret = false;
                        dumpLog.WriteLine("Output format does not support USB descriptors.");
                        DicConsole.ErrorWriteLine("Output format does not support USB descriptors.");
                    }

                    if(dev.IsPcmcia && dev.Cis != null &&
                       !outputPlugin.SupportedMediaTags.Contains(MediaTagType.PCMCIA_CIS))
                    {
                        ret = false;
                        dumpLog.WriteLine("Output format does not support PCMCIA CIS descriptors.");
                        DicConsole.ErrorWriteLine("Output format does not support PCMCIA CIS descriptors.");
                    }

                    if(!outputPlugin.SupportedMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
                    {
                        ret = false;
                        dumpLog.WriteLine("Output format does not support ATA IDENTIFY.");
                        DicConsole.ErrorWriteLine("Output format does not support ATA IDENTIFY.");
                    }

                    if(!ret)
                    {
                        dumpLog.WriteLine("Several media tags not supported, {0}continuing...", force ? "" : "not ");
                        DicConsole.ErrorWriteLine("Several media tags not supported, {0}continuing...",
                                                  force ? "" : "not ");
                        if(!force) return;
                    }

                    ret = outputPlugin.Create(outputPath,
                                              dev.IsCompactFlash ? MediaType.CompactFlash : MediaType.GENERIC_HDD,
                                              formatOptions, blocks, blockSize);

                    // Cannot create image
                    if(!ret)
                    {
                        dumpLog.WriteLine("Error creating output image, not continuing.");
                        dumpLog.WriteLine(outputPlugin.ErrorMessage);
                        DicConsole.ErrorWriteLine("Error creating output image, not continuing.");
                        DicConsole.ErrorWriteLine(outputPlugin.ErrorMessage);
                        return;
                    }

                    // Setting geometry
                    outputPlugin.SetGeometry(cylinders, heads, sectors);

                    if(ataReader.IsLba)
                    {
                        DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);
                        if(skip < blocksToRead) skip = blocksToRead;

                        mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
                        ibgLog  = new IbgLog(outputPrefix  + ".ibg", ATA_PROFILE);

                        if(resume.NextBlock > 0) dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);
                        bool newTrim = false;

                        start = DateTime.UtcNow;
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

                            if(blocks - i < blocksToRead) blocksToRead = (byte)(blocks - i);

                            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                            if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                            #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

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
                        DicConsole.WriteLine();
                        mhddLog.Close();
                        ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                     blockSize * (double)(blocks + 1) / 1024 /
                                     (totalDuration / 1000), devicePath);
                        dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
                        dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                                          (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));
                        dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                                          (double)blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration);

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

                                bool error = ataReader.ReadBlock(out cmdBuf, badSector, out duration);

                                totalDuration += duration;

                                if(error) continue;

                                resume.BadBlocks.Remove(badSector);
                                extents.Add(badSector);
                                outputPlugin.WriteSector(cmdBuf, badSector);
                            }

                            DicConsole.WriteLine();
                            end = DateTime.UtcNow;
                            dumpLog.WriteLine("Trimmming finished in {0} seconds.", (end - start).TotalSeconds);
                        }
                        #endregion Trimming

                        #region Error handling
                        if(resume.BadBlocks.Count > 0 && !aborted && retryPasses > 0)
                        {
                            int  pass    = 1;
                            bool forward = true;

                            repeatRetryLba:
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
                                                 persistent ? "recovering partial data, " : "");

                                bool error = ataReader.ReadBlock(out cmdBuf, badSector, out duration);

                                totalDuration += duration;

                                if(!error)
                                {
                                    resume.BadBlocks.Remove(badSector);
                                    extents.Add(badSector);
                                    outputPlugin.WriteSector(cmdBuf, badSector);
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

                            DicConsole.WriteLine();
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
                        for(ushort cy = 0; cy < cylinders; cy++)
                        {
                            for(byte hd = 0; hd < heads; hd++)
                            {
                                for(byte sc = 1; sc < sectors; sc++)
                                {
                                    if(aborted)
                                    {
                                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                        dumpLog.WriteLine("Aborted!");
                                        break;
                                    }

                                    #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                                    if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                                    if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                                    #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                                    DicConsole.Write("\rReading cylinder {0} head {1} sector {2} ({3:F3} MiB/sec.)", cy,
                                                     hd, sc, currentSpeed);

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
                        DicConsole.WriteLine();
                        mhddLog.Close();
                        ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                     blockSize * (double)(blocks + 1) / 1024 /
                                     (totalDuration / 1000), devicePath);
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
                        if(preSidecar != null)
                        {
                            preSidecar.BlockMedia = sidecar.BlockMedia;
                            sidecar               = preSidecar;
                        }

                        if(dev.IsUsb)
                        {
                            dumpLog.WriteLine("Reading USB descriptors.");
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
                                dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type,
                                                  filesystem.start);

                        DicConsole.WriteLine();
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

                        DicConsole.WriteLine("Writing metadata sidecar");

                        FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml", FileMode.Create);

                        XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                        xmlSer.Serialize(xmlFs, sidecar);
                        xmlFs.Close();
                    }

                    DicConsole.WriteLine();

                    DicConsole
                       .WriteLine("Took a total of {0:F3} seconds ({1:F3} processing commands, {2:F3} checksumming, {3:F3} writing, {4:F3} closing).",
                                  (end - start).TotalSeconds, totalDuration / 1000, totalChkDuration / 1000,
                                  imageWriteDuration,         (closeEnd - closeStart).TotalSeconds);
                    DicConsole.WriteLine("Average speed: {0:F3} MiB/sec.",
                                         (double)blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000));
                    DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", maxSpeed);
                    DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", minSpeed);
                    DicConsole.WriteLine("{0} sectors could not be read.",       resume.BadBlocks.Count);
                    if(resume.BadBlocks.Count > 0) resume.BadBlocks.Sort();
                    DicConsole.WriteLine();
                }

                if(dev.IsCompactFlash) Statistics.AddMedia(MediaType.CompactFlash, true);
                else if(dev.IsPcmcia) Statistics.AddMedia(MediaType.PCCardTypeI,   true);
                else Statistics.AddMedia(MediaType.GENERIC_HDD,                    true);
            }
            else DicConsole.ErrorWriteLine("Unable to communicate with ATA device.");
        }
    }
}