// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SecureDigital.cs
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
using DiscImageChef.Decoders.MMC;
using DiscImageChef.Devices;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using Extents;
using Schemas;

namespace DiscImageChef.Core.Devices.Dumping
{
    public class SecureDigital
    {
        public static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force, bool dumpRaw, bool persistent, bool stopOnError, ref Metadata.Resume resume)
        {
            bool aborted;
            MHDDLog mhddLog;
            IBGLog ibgLog;

            if(dumpRaw)
            {
                DicConsole.ErrorWriteLine("Raw dumping is not supported in MultiMediaCard or SecureDigital devices.");

                if(force)
                    DicConsole.ErrorWriteLine("Continuing...");
                else
                {
                    DicConsole.ErrorWriteLine("Aborting...");
                    return;
                }
            }

            bool sense;
            ushort currentProfile = 0x0001;
            uint timeout = 5;
            double duration;

            CICMMetadataType sidecar = new CICMMetadataType()
            {
                BlockMedia = new BlockMediaType[] { new BlockMediaType() }
            };

            sidecar.BlockMedia[0].SecureDigital = new SecureDigitalType();

            uint blocksToRead = 128;
            uint blockSize = 512;
            ulong blocks = 0;
            byte[] cid = null;
            byte[] csd = null;
            byte[] ocr = null;
            byte[] ecsd = null;
            byte[] scr = null;
            uint[] response;
            int physicalBlockSize = 0;
            bool byteAddressed = true;

            if(dev.Type == DeviceType.MMC)
            {
                ExtendedCSD ecsdDecoded = new ExtendedCSD();
                CSD csdDecoded = new CSD();

                sense = dev.ReadExtendedCSD(out ecsd, out response, timeout, out duration);
                if(!sense)
                {
                    ecsdDecoded = Decoders.MMC.Decoders.DecodeExtendedCSD(ecsd);
                    blocksToRead = ecsdDecoded.OptimalReadSize;
                    blocks = ecsdDecoded.SectorCount;
                    blockSize = (uint)(ecsdDecoded.SectorSize == 1 ? 4096 : 512);
                    if(ecsdDecoded.NativeSectorSize == 0)
                        physicalBlockSize = 512;
                    else if(ecsdDecoded.NativeSectorSize == 1)
                        physicalBlockSize = 4096;
                    // Supposing it's high-capacity MMC if it has Extended CSD...
                    byteAddressed = false;
                }
                else
                    ecsd = null;

                sense = dev.ReadCSD(out csd, out response, timeout, out duration);
                if(!sense)
                {
                    if(blocks == 0)
                    {
                        csdDecoded = Decoders.MMC.Decoders.DecodeCSD(csd);
                        blocks = (ulong)((csdDecoded.Size + 1) * Math.Pow(2, csdDecoded.SizeMultiplier + 2));
                        blockSize = (uint)Math.Pow(2, csdDecoded.ReadBlockLength);
                    }
                }
                else
                    csd = null;

                sense = dev.ReadOCR(out ocr, out response, timeout, out duration);
                if(sense)
                    ocr = null;
            }
            else if(dev.Type == DeviceType.SecureDigital)
            {
                Decoders.SecureDigital.CSD csdDecoded = new Decoders.SecureDigital.CSD();

                sense = dev.ReadCSD(out csd, out response, timeout, out duration);
                if(!sense)
                {
                    csdDecoded = Decoders.SecureDigital.Decoders.DecodeCSD(csd);
                    blocks = (ulong)(csdDecoded.Structure == 0 ? (csdDecoded.Size + 1) * Math.Pow(2, csdDecoded.SizeMultiplier + 2) : (csdDecoded.Size + 1) * 1024);
                    blockSize = (uint)Math.Pow(2, csdDecoded.ReadBlockLength);
                    // Structure >=1 for SDHC/SDXC, so that's block addressed
                    byteAddressed = csdDecoded.Structure == 0;
                }
                else
                    csd = null;

                sense = dev.ReadSDOCR(out ocr, out response, timeout, out duration);
                if(sense)
                    ocr = null;

                sense = dev.ReadSCR(out scr, out response, timeout, out duration);
                if(sense)
                    scr = null;
            }

            sense = dev.ReadCID(out cid, out response, timeout, out duration);
            if(sense)
                cid = null;

            // TODO: MultiMediaCard should be different
            if(cid != null)
            {
                sidecar.BlockMedia[0].SecureDigital.CID = new DumpType
                {
                    Image = outputPrefix + ".cid.bin",
                    Size = cid.Length,
                    Checksums = Checksum.GetChecksums(cid).ToArray()
                };
                DataFile.WriteTo("MMC/SecureDigital Dump", sidecar.BlockMedia[0].SecureDigital.CID.Image, cid);
            };
            if(csd != null)
            {
                sidecar.BlockMedia[0].SecureDigital.CSD = new DumpType
                {
                    Image = outputPrefix + ".csd.bin",
                    Size = csd.Length,
                    Checksums = Checksum.GetChecksums(csd).ToArray()
                };
                DataFile.WriteTo("MMC/SecureDigital Dump", sidecar.BlockMedia[0].SecureDigital.CSD.Image, csd);
            };
            if(ecsd != null)
            {
                sidecar.BlockMedia[0].SecureDigital.ExtendedCSD = new DumpType
                {
                    Image = outputPrefix + ".ecsd.bin",
                    Size = ecsd.Length,
                    Checksums = Checksum.GetChecksums(ecsd).ToArray()
                };
                DataFile.WriteTo("MMC/SecureDigital Dump", sidecar.BlockMedia[0].SecureDigital.ExtendedCSD.Image, ecsd);
            };
            if(ocr != null)
            {
                // TODO: Add to metadata.xml
                /*sidecar.BlockMedia[0].SecureDigital.OCR = new DumpType
                {
                    Image = outputPrefix + ".ocr.bin",
                    Size = ocr.Length,
                    Checksums = Checksum.GetChecksums(ocr).ToArray()
                };
                DataFile.WriteTo("MMC/SecureDigital Dump", sidecar.BlockMedia[0].SecureDigital.OCR.Image, ocr);*/
                DataFile.WriteTo("MMC/SecureDigital Dump", outputPrefix + ".ocr.bin", ocr);
            };
            if(scr != null)
            {
                // TODO: Add to metadata.xml
                /*sidecar.BlockMedia[0].SecureDigital.SCR = new DumpType
                {
                    Image = outputPrefix + ".scr.bin",
                    Size = scr.Length,
                    Checksums = Checksum.GetChecksums(scr).ToArray()
                };
                DataFile.WriteTo("MMC/SecureDigital Dump", sidecar.BlockMedia[0].SecureDigital.SCR.Image, scr);*/
                DataFile.WriteTo("MMC/SecureDigital Dump", outputPrefix + ".scr.bin", scr);
            };

            DateTime start;
            DateTime end;
            double totalDuration = 0;
            double totalChkDuration = 0;
            double currentSpeed = 0;
            double maxSpeed = double.MinValue;
            double minSpeed = double.MaxValue;
            Checksum dataChk;

            aborted = false;
            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = aborted = true;
            };

            DataFile dumpFile;

            if(blocks == 0)
            {
                DicConsole.ErrorWriteLine("Unable to get device size.");
                return;
            }

            byte[] cmdBuf;
            bool error = true;

            while(true)
            {
                error = dev.Read(out cmdBuf, out response, 0, blockSize, blocksToRead, byteAddressed, timeout, out duration);

                if(error)
                    blocksToRead /= 2;

                if(!error || blocksToRead == 1)
                    break;
            }

            if(error)
            {
                blocksToRead = 1;
                DicConsole.ErrorWriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                return;
            }


//            bool removable = false || (!dev.IsCompactFlash && ataId.GeneralConfiguration.HasFlag(Decoders.ATA.Identify.GeneralConfigurationBit.Removable));
            DumpHardwareType currentTry = null;
            ExtentsULong extents = null;
            ResumeSupport.Process(true, false, blocks, dev.Manufacturer, dev.Model, dev.Serial, dev.PlatformID, ref resume, ref currentTry, ref extents);
            if(currentTry == null || extents == null)
                throw new Exception("Could not process resume file, not continuing...");

                DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

                mhddLog = new MHDDLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
                ibgLog = new IBGLog(outputPrefix + ".ibg", currentProfile);
                dumpFile = new DataFile(outputPrefix + ".bin");
                dumpFile.Seek(resume.NextBlock, blockSize);

                start = DateTime.UtcNow;
                for(ulong i = resume.NextBlock; i < blocks; i += blocksToRead)
                {
                    if(aborted)
                    {
                        currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);
                        break;
                    }

                    if((blocks - i) < blocksToRead)
                        blocksToRead = (byte)(blocks - i);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0)
                        maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0)
                        minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

                error = dev.Read(out cmdBuf, out response, (uint)i, blockSize, blocksToRead, byteAddressed, timeout, out duration);

                    if(!error)
                    {
                        mhddLog.Write(i, duration);
                        ibgLog.Write(i, currentSpeed * 1024);
                        dumpFile.Write(cmdBuf);
                        extents.Add(i, blocksToRead, true);
                    }
                    else
                    {
                        for(ulong b = i; b < i + blocksToRead; b++)
                            resume.BadBlocks.Add(b);
                        if(duration < 500)
                            mhddLog.Write(i, 65535);
                        else
                            mhddLog.Write(i, duration);

                        ibgLog.Write(i, 0);
                        dumpFile.Write(new byte[blockSize * blocksToRead]);
                    }

#pragma warning disable IDE0004 // Cast is necessary, otherwise incorrect value is created
                    currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (duration / (double)1000);
#pragma warning restore IDE0004 // Cast is necessary, otherwise incorrect value is created
                    GC.Collect();
                    resume.NextBlock = i + blocksToRead;
                }
                end = DateTime.Now;
                DicConsole.WriteLine();
                mhddLog.Close();
#pragma warning disable IDE0004 // Cast is necessary, otherwise incorrect value is created
                ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);
#pragma warning restore IDE0004 // Cast is necessary, otherwise incorrect value is created

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
                            currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);
                            break;
                        }

                        DicConsole.Write("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass + 1, forward ? "forward" : "reverse", runningPersistent ? "recovering partial data, " : "");

                    error = dev.Read(out cmdBuf, out response, (uint)badSector, blockSize, 1, byteAddressed, timeout, out duration);

                        totalDuration += duration;

                        if(!error)
                        {
                            resume.BadBlocks.Remove(badSector);
                            extents.Add(badSector);
                            dumpFile.WriteAt(cmdBuf, badSector, blockSize);
                        }
                        else if(runningPersistent)
                            dumpFile.WriteAt(cmdBuf, badSector, blockSize);
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
                #endregion Error handling

                currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);

                dataChk = new Checksum();
            dumpFile.Seek(0, SeekOrigin.Begin);
            blocksToRead = 500;

            for(ulong i = 0; i < blocks; i += blocksToRead)
            {
                if(aborted)
                    break;

                if((blocks - i) < blocksToRead)
                    blocksToRead = (byte)(blocks - i);

                DicConsole.Write("\rChecksumming sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

                DateTime chkStart = DateTime.UtcNow;
                byte[] dataToCheck = new byte[blockSize * blocksToRead];
                dumpFile.Read(dataToCheck, 0, (int)(blockSize * blocksToRead));
                dataChk.Update(dataToCheck);
                DateTime chkEnd = DateTime.UtcNow;

                double chkDuration = (chkEnd - chkStart).TotalMilliseconds;
                totalChkDuration += chkDuration;

                currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (chkDuration / (double)1000);
            }
            DicConsole.WriteLine();
            dumpFile.Close();
            end = DateTime.UtcNow;

            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();
            ImagePlugin _imageFormat;

            FiltersList filtersList = new FiltersList();
            Filter inputFilter = filtersList.GetFilter(outputPrefix + ".bin");

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open file just created, this should not happen.");
                return;
            }

            _imageFormat = ImageFormat.Detect(inputFilter);
            PartitionType[] xmlFileSysInfo = null;

            try
            {
                if(!_imageFormat.OpenImage(inputFilter))
                    _imageFormat = null;
            }
            catch
            {
                _imageFormat = null;
            }

            if(_imageFormat != null)
            {
                List<Partition> partitions = Partitions.GetAll(_imageFormat);
                Partitions.AddSchemesToStats(partitions);

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

                        foreach(Filesystem _plugin in plugins.PluginsList.Values)
                        {
                            try
                            {
                                if(_plugin.Identify(_imageFormat, partitions[i]))
                                {
                                    _plugin.GetInformation(_imageFormat, partitions[i], out string foo);
                                    lstFs.Add(_plugin.XmlFSType);
                                    Statistics.AddFilesystem(_plugin.XmlFSType.Type);
                                }
                            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                            catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                            {
                                //DicConsole.DebugWriteLine("Dump-media command", "Plugin {0} crashed", _plugin.Name);
                            }
                        }

                        if(lstFs.Count > 0)
                            xmlFileSysInfo[i].FileSystems = lstFs.ToArray();
                    }
                }
                else
                {
                    xmlFileSysInfo = new PartitionType[1];
                    xmlFileSysInfo[0] = new PartitionType
                    {
                        EndSector = (int)(blocks - 1),
                        StartSector = 0
                    };
                    List<FileSystemType> lstFs = new List<FileSystemType>();

                    Partition wholePart = new Partition
                    {
                        Name = "Whole device",
                        Length = blocks,
                        Size = blocks * blockSize
                    };

                    foreach(Filesystem _plugin in plugins.PluginsList.Values)
                    {
                        try
                        {
                            if(_plugin.Identify(_imageFormat, wholePart))
                            {
                                _plugin.GetInformation(_imageFormat, wholePart, out string foo);
                                lstFs.Add(_plugin.XmlFSType);
                                Statistics.AddFilesystem(_plugin.XmlFSType.Type);
                            }
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {
                            //DicConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                        }
                    }

                    if(lstFs.Count > 0)
                        xmlFileSysInfo[0].FileSystems = lstFs.ToArray();
                }
            }

            sidecar.BlockMedia[0].Checksums = dataChk.End().ToArray();
            string xmlDskTyp = null, xmlDskSubTyp = null;
            if(dev.Type == DeviceType.MMC)
            {
                Metadata.MediaType.MediaTypeToString(MediaType.MMC, out xmlDskTyp, out xmlDskSubTyp);
                sidecar.BlockMedia[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(MediaType.MMC);
            }
            else if(dev.Type == DeviceType.SecureDigital)
            {
                Metadata.MediaType.MediaTypeToString(MediaType.SecureDigital, out xmlDskTyp, out xmlDskSubTyp);
                sidecar.BlockMedia[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(MediaType.SecureDigital);
            }
            sidecar.BlockMedia[0].DiskType = xmlDskTyp;
            sidecar.BlockMedia[0].DiskSubType = xmlDskSubTyp;
            // TODO: Implement device firmware revision
            sidecar.BlockMedia[0].Image = new ImageType
            {
                format = "Raw disk image (sector by sector copy)",
                Value = outputPrefix + ".bin"
            };
            if(dev.Type == DeviceType.MMC)
                sidecar.BlockMedia[0].Interface = "MultiMediaCard";
            else if(dev.Type == DeviceType.SecureDigital)
                sidecar.BlockMedia[0].Interface = "SecureDigital";
            sidecar.BlockMedia[0].LogicalBlocks = (long)blocks;
            sidecar.BlockMedia[0].PhysicalBlockSize = physicalBlockSize > 0 ? physicalBlockSize : (int)blockSize;
            sidecar.BlockMedia[0].LogicalBlockSize = (int)blockSize;
            sidecar.BlockMedia[0].Manufacturer = dev.Manufacturer;
            sidecar.BlockMedia[0].Model = dev.Model;
            sidecar.BlockMedia[0].Serial = dev.Serial;
            sidecar.BlockMedia[0].Size = (long)(blocks * blockSize);
            if(xmlFileSysInfo != null)
                sidecar.BlockMedia[0].FileSystemInformation = xmlFileSysInfo;

            DicConsole.WriteLine();

            DicConsole.WriteLine("Took a total of {0:F3} seconds ({1:F3} processing commands, {2:F3} checksumming).", (end - start).TotalSeconds, totalDuration / 1000, totalChkDuration / 1000);
            DicConsole.WriteLine("Avegare speed: {0:F3} MiB/sec.", (((double)blockSize * (double)(blocks + 1)) / 1048576) / (totalDuration / 1000));
            DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", maxSpeed);
            DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", minSpeed);
            DicConsole.WriteLine("{0} sectors could not be read.", resume.BadBlocks.Count);
            if(resume.BadBlocks.Count > 0)
                resume.BadBlocks.Sort();
            DicConsole.WriteLine();

            if(!aborted)
            {
                DicConsole.WriteLine("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml",
                                              FileMode.Create);

                System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }

            if(dev.Type == DeviceType.MMC)
                Statistics.AddMedia(MediaType.MMC, true);
            else if(dev.Type == DeviceType.SecureDigital)
                Statistics.AddMedia(MediaType.SecureDigital, true);
        }
    }
}
