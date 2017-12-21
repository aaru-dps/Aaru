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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Devices;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.Metadata;
using Extents;
using Schemas;
using MediaType = DiscImageChef.Metadata.MediaType;
using Tuple = DiscImageChef.Decoders.PCMCIA.Tuple;

namespace DiscImageChef.Core.Devices.Dumping
{
    public class Ata
    {
        public static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force,
                                bool dumpRaw, bool persistent, bool stopOnError, ref Resume resume,
                                ref DumpLog dumpLog, Encoding encoding)
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

            bool sense;
            const ushort ATA_PROFILE = 0x0001;
            const uint TIMEOUT = 5;

            dumpLog.WriteLine("Requesting ATA IDENTIFY DEVICE.");
            sense = dev.AtaIdentify(out byte[] cmdBuf, out _);
            if(!sense && Identify.Decode(cmdBuf).HasValue)
            {
                Identify.IdentifyDevice? ataIdNullable = Identify.Decode(cmdBuf);
                if(ataIdNullable != null) {
                    Identify.IdentifyDevice ataId = ataIdNullable.Value;

                    CICMMetadataType sidecar =
                        new CICMMetadataType {BlockMedia = new[] {new BlockMediaType()}};

                    if(dev.IsUsb)
                    {
                        dumpLog.WriteLine("Reading USB descriptors.");
                        sidecar.BlockMedia[0].USB = new USBType
                        {
                            ProductID = dev.UsbProductId,
                            VendorID = dev.UsbVendorId,
                            Descriptors = new DumpType
                            {
                                Image = outputPrefix + ".usbdescriptors.bin",
                                Size = dev.UsbDescriptors.Length,
                                Checksums = Checksum.GetChecksums(dev.UsbDescriptors).ToArray()
                            }
                        };
                        DataFile.WriteTo("ATA Dump", sidecar.BlockMedia[0].USB.Descriptors.Image, dev.UsbDescriptors);
                    }

                    if(dev.IsPcmcia)
                    {
                        dumpLog.WriteLine("Reading PCMCIA CIS.");
                        sidecar.BlockMedia[0].PCMCIA = new PCMCIAType
                        {
                            CIS = new DumpType
                            {
                                Image = outputPrefix + ".cis.bin",
                                Size = dev.Cis.Length,
                                Checksums = Checksum.GetChecksums(dev.Cis).ToArray()
                            }
                        };
                        DataFile.WriteTo("ATA Dump", sidecar.BlockMedia[0].PCMCIA.CIS.Image, dev.Cis);
                        dumpLog.WriteLine("Decoding PCMCIA CIS.");
                        Tuple[] tuples = CIS.GetTuples(dev.Cis);
                        if(tuples != null)
                            foreach(Tuple tuple in tuples)
                                switch(tuple.Code) {
                                    case TupleCodes.CISTPL_MANFID:
                                        ManufacturerIdentificationTuple manfid =
                                            CIS.DecodeManufacturerIdentificationTuple(tuple);

                                        if(manfid != null)
                                        {
                                            sidecar.BlockMedia[0].PCMCIA.ManufacturerCode = manfid.ManufacturerID;
                                            sidecar.BlockMedia[0].PCMCIA.CardCode = manfid.CardID;
                                            sidecar.BlockMedia[0].PCMCIA.ManufacturerCodeSpecified = true;
                                            sidecar.BlockMedia[0].PCMCIA.CardCodeSpecified = true;
                                        }
                                        break;
                                    case TupleCodes.CISTPL_VERS_1:
                                        Level1VersionTuple vers = CIS.DecodeLevel1VersionTuple(tuple);

                                        if(vers != null)
                                        {
                                            sidecar.BlockMedia[0].PCMCIA.Manufacturer = vers.Manufacturer;
                                            sidecar.BlockMedia[0].PCMCIA.ProductName = vers.Product;
                                            sidecar.BlockMedia[0].PCMCIA.Compliance =
                                                $"{vers.MajorVersion}.{vers.MinorVersion}";
                                            sidecar.BlockMedia[0].PCMCIA.AdditionalInformation = vers.AdditionalInformation;
                                        }
                                        break;
                                }
                    }

                    sidecar.BlockMedia[0].ATA = new ATAType
                    {
                        Identify = new DumpType
                        {
                            Image = outputPrefix + ".identify.bin",
                            Size = cmdBuf.Length,
                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                        }
                    };
                    DataFile.WriteTo("ATA Dump", sidecar.BlockMedia[0].ATA.Identify.Image, cmdBuf);

                    DateTime start;
                    DateTime end;
                    double totalDuration = 0;
                    double totalChkDuration = 0;
                    double currentSpeed = 0;
                    double maxSpeed = double.MinValue;
                    double minSpeed = double.MaxValue;

                    aborted = false;
                    System.Console.CancelKeyPress += (sender, e) => e.Cancel = aborted = true;

                    DataFile dumpFile;

                    // Initializate reader
                    dumpLog.WriteLine("Initializing reader.");
                    Reader ataReader = new Reader(dev, TIMEOUT, cmdBuf);
                    // Fill reader blocks
                    ulong blocks = ataReader.GetDeviceBlocks();
                    // Check block sizes
                    if(ataReader.GetBlockSize())
                    {
                        dumpLog.WriteLine("ERROR: Cannot get block size: {0}.", ataReader.ErrorMessage);
                        DicConsole.ErrorWriteLine(ataReader.ErrorMessage);
                        return;
                    }

                    uint blockSize = ataReader.LogicalBlockSize;
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

                    uint blocksToRead = ataReader.BlocksToRead;
                    ushort cylinders = ataReader.Cylinders;
                    byte heads = ataReader.Heads;
                    byte sectors = ataReader.Sectors;

                    dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).", blocks, blocks * blockSize);
                    dumpLog.WriteLine("Device reports {0} cylinders {1} heads {2} sectors per track.", cylinders, heads,
                                      sectors);
                    dumpLog.WriteLine("Device can read {0} blocks at a time.", blocksToRead);
                    dumpLog.WriteLine("Device reports {0} bytes per logical block.", blockSize);
                    dumpLog.WriteLine("Device reports {0} bytes per physical block.", physicalsectorsize);

                    bool removable = !dev.IsCompactFlash &&
                                     ataId.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit
                                                                                .Removable);
                    DumpHardwareType currentTry = null;
                    ExtentsULong extents = null;
                    ResumeSupport.Process(ataReader.IsLba, removable, blocks, dev.Manufacturer, dev.Model, dev.Serial,
                                          dev.PlatformId, ref resume, ref currentTry, ref extents);
                    if(currentTry == null || extents == null)
                        throw new Exception("Could not process resume file, not continuing...");

                    MhddLog mhddLog;
                    IbgLog ibgLog;
                    double duration;
                    if(ataReader.IsLba)
                    {
                        DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

                        mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
                        ibgLog = new IbgLog(outputPrefix + ".ibg", ATA_PROFILE);
                        dumpFile = new DataFile(outputPrefix + ".bin");
                        if(resume.NextBlock > 0) dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);

                        dumpFile.Seek(resume.NextBlock, blockSize);

                        start = DateTime.UtcNow;
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
                                dumpFile.Write(cmdBuf);
                                extents.Add(i, blocksToRead, true);
                            }
                            else
                            {
                                for(ulong b = i; b < i + blocksToRead; b++) resume.BadBlocks.Add(b);

                                mhddLog.Write(i, duration < 500 ? 65535 : duration);

                                ibgLog.Write(i, 0);
                                dumpFile.Write(new byte[blockSize * blocksToRead]);
                                dumpLog.WriteLine("Error reading {0} blocks from block {1}.", blocksToRead, i);
                            }

                            currentSpeed = (double)blockSize * blocksToRead / 1048576 / (duration / 1000);
                            GC.Collect();
                            resume.NextBlock = i + blocksToRead;
                        }

                        end = DateTime.Now;
                        DicConsole.WriteLine();
                        mhddLog.Close();
                        ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                     blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000),
                                     devicePath);
                        dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
                        dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                                          (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));

                        #region Error handling
                        if(resume.BadBlocks.Count > 0 && !aborted)
                        {
                            int pass = 0;
                            bool forward = true;
                            bool runningPersistent = false;

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

                                DicConsole.Write("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass + 1,
                                                 forward ? "forward" : "reverse",
                                                 runningPersistent ? "recovering partial data, " : "");

                                bool error = ataReader.ReadBlock(out cmdBuf, badSector, out duration);

                                totalDuration += duration;

                                if(!error)
                                {
                                    resume.BadBlocks.Remove(badSector);
                                    extents.Add(badSector);
                                    dumpFile.WriteAt(cmdBuf, badSector, blockSize);
                                    dumpLog.WriteLine("Correctly retried block {0} in pass {1}.", badSector, pass);
                                }
                                else if(runningPersistent) dumpFile.WriteAt(cmdBuf, badSector, blockSize);
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
                        ibgLog = new IbgLog(outputPrefix + ".ibg", ATA_PROFILE);
                        dumpFile = new DataFile(outputPrefix + ".bin");

                        ulong currentBlock = 0;
                        blocks = (ulong)(cylinders * heads * sectors);
                        start = DateTime.UtcNow;
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

                                    DicConsole.Write("\rReading cylinder {0} head {1} sector {2} ({3:F3} MiB/sec.)", cy, hd,
                                                     sc, currentSpeed);

                                    bool error = ataReader.ReadChs(out cmdBuf, cy, hd, sc, out duration);

                                    totalDuration += duration;

                                    if(!error)
                                    {
                                        mhddLog.Write(currentBlock, duration);
                                        ibgLog.Write(currentBlock, currentSpeed * 1024);
                                        dumpFile.Write(cmdBuf);
                                        extents.Add(currentBlock);
                                        dumpLog.WriteLine("Error reading cylinder {0} head {1} sector {2}.", cy, hd, sc);
                                    }
                                    else
                                    {
                                        resume.BadBlocks.Add(currentBlock);
                                        mhddLog.Write(currentBlock, duration < 500 ? 65535 : duration);

                                        ibgLog.Write(currentBlock, 0);
                                        dumpFile.Write(new byte[blockSize]);
                                    }

                                    currentSpeed = blockSize / (double)1048576 / (duration / 1000);
                                    GC.Collect();

                                    currentBlock++;
                                }
                            }
                        }

                        end = DateTime.Now;
                        DicConsole.WriteLine();
                        mhddLog.Close();
                        ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                     blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000),
                                     devicePath);
                        dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
                        dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                                          (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));
                    }

                    Checksum dataChk = new Checksum();
                    dumpFile.Seek(0, SeekOrigin.Begin);
                    blocksToRead = 500;

                    dumpLog.WriteLine("Checksum starts.");
                    for(ulong i = 0; i < blocks; i += blocksToRead)
                    {
                        if(aborted)
                        {
                            dumpLog.WriteLine("Aborted!");
                            break;
                        }

                        if(blocks - i < blocksToRead) blocksToRead = (byte)(blocks - i);

                        DicConsole.Write("\rChecksumming sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

                        DateTime chkStart = DateTime.UtcNow;
                        byte[] dataToCheck = new byte[blockSize * blocksToRead];
                        dumpFile.Read(dataToCheck, 0, (int)(blockSize * blocksToRead));
                        dataChk.Update(dataToCheck);
                        DateTime chkEnd = DateTime.UtcNow;

                        double chkDuration = (chkEnd - chkStart).TotalMilliseconds;
                        totalChkDuration += chkDuration;

                        currentSpeed = (double)blockSize * blocksToRead / 1048576 / (chkDuration / 1000);
                    }

                    DicConsole.WriteLine();
                    dumpFile.Close();
                    end = DateTime.UtcNow;
                    dumpLog.WriteLine("Checksum finished in {0} seconds.", (end - start).TotalSeconds);
                    dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                                      (double)blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                    PluginBase plugins = new PluginBase();
                    plugins.RegisterAllPlugins(encoding);

                    FiltersList filtersList = new FiltersList();
                    Filter inputFilter = filtersList.GetFilter(outputPrefix + ".bin");

                    if(inputFilter == null)
                    {
                        DicConsole.ErrorWriteLine("Cannot open file just created, this should not happen.");
                        return;
                    }

                    ImagePlugin imageFormat = ImageFormat.Detect(inputFilter);
                    PartitionType[] xmlFileSysInfo = null;

                    try { if(!imageFormat.OpenImage(inputFilter)) imageFormat = null; }
                    catch { imageFormat = null; }

                    if(imageFormat != null)
                    {
                        dumpLog.WriteLine("Getting partitions.");
                        List<Partition> partitions = Partitions.GetAll(imageFormat);
                        Partitions.AddSchemesToStats(partitions);
                        dumpLog.WriteLine("Found {0} partitions.", partitions.Count);

                        if(partitions.Count > 0)
                        {
                            xmlFileSysInfo = new PartitionType[partitions.Count];
                            for(int i = 0; i < partitions.Count; i++)
                            {
                                xmlFileSysInfo[i] = new PartitionType
                                {
                                    Description = partitions[i].Description,
                                    EndSector = (int)(partitions[i].Start + partitions[i].Length - 1),
                                    Name = partitions[i].Name,
                                    Sequence = (int)partitions[i].Sequence,
                                    StartSector = (int)partitions[i].Start,
                                    Type = partitions[i].Type
                                };
                                List<FileSystemType> lstFs = new List<FileSystemType>();
                                dumpLog
                                    .WriteLine("Getting filesystems on partition {0}, starting at {1}, ending at {2}, with type {3}, under scheme {4}.",
                                               i, partitions[i].Start, partitions[i].End, partitions[i].Type,
                                               partitions[i].Scheme);

                                foreach(Filesystem plugin in plugins.PluginsList.Values)
                                    try
                                    {
                                        if(!plugin.Identify(imageFormat, partitions[i])) continue;

                                        plugin.GetInformation(imageFormat, partitions[i], out _);
                                        lstFs.Add(plugin.XmlFSType);
                                        Statistics.AddFilesystem(plugin.XmlFSType.Type);
                                        dumpLog.WriteLine("Filesystem {0} found.", plugin.XmlFSType.Type);
                                    }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                    catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                    {
                                        //DicConsole.DebugWriteLine("Dump-media command", "Plugin {0} crashed", _plugin.Name);
                                    }

                                if(lstFs.Count > 0) xmlFileSysInfo[i].FileSystems = lstFs.ToArray();
                            }
                        }
                        else
                        {
                            dumpLog.WriteLine("Getting filesystem for whole device.");

                            xmlFileSysInfo = new PartitionType[1];
                            xmlFileSysInfo[0] = new PartitionType {EndSector = (int)(blocks - 1), StartSector = 0};
                            List<FileSystemType> lstFs = new List<FileSystemType>();

                            Partition wholePart = new Partition
                            {
                                Name = "Whole device",
                                Length = blocks,
                                Size = blocks * blockSize
                            };

                            foreach(Filesystem plugin in plugins.PluginsList.Values)
                                try
                                {
                                    if(!plugin.Identify(imageFormat, wholePart)) continue;

                                    plugin.GetInformation(imageFormat, wholePart, out _);
                                    lstFs.Add(plugin.XmlFSType);
                                    Statistics.AddFilesystem(plugin.XmlFSType.Type);
                                    dumpLog.WriteLine("Filesystem {0} found.", plugin.XmlFSType.Type);
                                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                {
                                    //DicConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                                }

                            if(lstFs.Count > 0) xmlFileSysInfo[0].FileSystems = lstFs.ToArray();
                        }
                    }

                    sidecar.BlockMedia[0].Checksums = dataChk.End().ToArray();
                    string xmlDskTyp, xmlDskSubTyp;
                    if(dev.IsCompactFlash)
                        MediaType.MediaTypeToString(CommonTypes.MediaType.CompactFlash, out xmlDskTyp, out xmlDskSubTyp);
                    else if(dev.IsPcmcia)
                        MediaType.MediaTypeToString(CommonTypes.MediaType.PCCardTypeI, out xmlDskTyp, out xmlDskSubTyp);
                    else MediaType.MediaTypeToString(CommonTypes.MediaType.GENERIC_HDD, out xmlDskTyp, out xmlDskSubTyp);
                    sidecar.BlockMedia[0].DiskType = xmlDskTyp;
                    sidecar.BlockMedia[0].DiskSubType = xmlDskSubTyp;
                    // TODO: Implement device firmware revision
                    sidecar.BlockMedia[0].Image = new ImageType
                    {
                        format = "Raw disk image (sector by sector copy)",
                        Value = outputPrefix + ".bin"
                    };
                    sidecar.BlockMedia[0].Interface = "ATA";
                    sidecar.BlockMedia[0].LogicalBlocks = (long)blocks;
                    sidecar.BlockMedia[0].PhysicalBlockSize = (int)physicalsectorsize;
                    sidecar.BlockMedia[0].LogicalBlockSize = (int)blockSize;
                    sidecar.BlockMedia[0].Manufacturer = dev.Manufacturer;
                    sidecar.BlockMedia[0].Model = dev.Model;
                    sidecar.BlockMedia[0].Serial = dev.Serial;
                    sidecar.BlockMedia[0].Size = (long)(blocks * blockSize);
                    if(xmlFileSysInfo != null) sidecar.BlockMedia[0].FileSystemInformation = xmlFileSysInfo;
                    if(cylinders > 0 && heads > 0 && sectors > 0)
                    {
                        sidecar.BlockMedia[0].Cylinders = cylinders;
                        sidecar.BlockMedia[0].CylindersSpecified = true;
                        sidecar.BlockMedia[0].Heads = heads;
                        sidecar.BlockMedia[0].HeadsSpecified = true;
                        sidecar.BlockMedia[0].SectorsPerTrack = sectors;
                        sidecar.BlockMedia[0].SectorsPerTrackSpecified = true;
                    }

                    DicConsole.WriteLine();

                    DicConsole.WriteLine("Took a total of {0:F3} seconds ({1:F3} processing commands, {2:F3} checksumming).",
                                         (end - start).TotalSeconds, totalDuration / 1000, totalChkDuration / 1000);
                    DicConsole.WriteLine("Avegare speed: {0:F3} MiB/sec.",
                                         (double)blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000));
                    DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", maxSpeed);
                    DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", minSpeed);
                    DicConsole.WriteLine("{0} sectors could not be read.", resume.BadBlocks.Count);
                    if(resume.BadBlocks.Count > 0) resume.BadBlocks.Sort();
                    DicConsole.WriteLine();

                    if(!aborted)
                    {
                        DicConsole.WriteLine("Writing metadata sidecar");

                        FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml", FileMode.Create);

                        XmlSerializer xmlSer =
                            new XmlSerializer(typeof(CICMMetadataType));
                        xmlSer.Serialize(xmlFs, sidecar);
                        xmlFs.Close();
                    }
                }

                Statistics.AddMedia(CommonTypes.MediaType.GENERIC_HDD, true);
            }
            else DicConsole.ErrorWriteLine("Unable to communicate with ATA device.");
        }
    }
}