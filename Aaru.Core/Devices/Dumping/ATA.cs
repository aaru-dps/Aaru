// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Core.Devices.Dumping;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Devices.Report;
using Aaru.Core.Logging;
using Aaru.Decoders.ATA;
using Aaru.Decoders.PCMCIA;
using Schemas;
using Identify = Aaru.CommonTypes.Structs.Devices.ATA.Identify;
using Tuple = Aaru.Decoders.PCMCIA.Tuple;
using Version = Aaru.CommonTypes.Interop.Version;

/// <summary>Implements dumping ATA devices</summary>
public partial class Dump
{
    /// <summary>Dumps an ATA device</summary>
    void Ata()
    {
        if(_outputPlugin is not IWritableImage outputFormat)
        {
            StoppingErrorMessage?.Invoke("Image is not writable, aborting...");

            return;
        }

        if(_dumpRaw)
        {
            if(_force)
                ErrorMessage?.Invoke("Raw dumping not yet supported in ATA devices, continuing...");
            else
            {
                StoppingErrorMessage?.Invoke("Raw dumping not yet supported in ATA devices, aborting...");

                return;
            }
        }

        const ushort ataProfile         = 0x0001;
        const uint   timeout            = 5;
        double       imageWriteDuration = 0;
        MediaType    mediaType          = MediaType.Unknown;

        UpdateStatus?.Invoke("Requesting ATA IDENTIFY DEVICE.");
        _dumpLog.WriteLine("Requesting ATA IDENTIFY DEVICE.");
        bool sense = _dev.AtaIdentify(out byte[] cmdBuf, out AtaErrorRegistersChs errorChs);

        if(sense)
            _errorLog?.WriteLine("ATA IDENTIFY DEVICE", _dev.Error, _dev.LastError, errorChs);
        else if(Identify.Decode(cmdBuf).HasValue)
        {
            Identify.IdentifyDevice? ataIdNullable = Identify.Decode(cmdBuf);

            if(ataIdNullable != null)
            {
                Identify.IdentifyDevice ataId       = ataIdNullable.Value;
                byte[]                  ataIdentify = cmdBuf;
                cmdBuf = Array.Empty<byte>();

                DateTime start;
                DateTime end;
                double   totalDuration = 0;
                double   currentSpeed  = 0;
                double   maxSpeed      = double.MinValue;
                double   minSpeed      = double.MaxValue;

                // Initialize reader
                UpdateStatus?.Invoke("Initializing reader.");
                _dumpLog.WriteLine("Initializing reader.");
                var ataReader = new Reader(_dev, timeout, ataIdentify, _errorLog);

                // Fill reader blocks
                ulong blocks = ataReader.GetDeviceBlocks();

                // Check block sizes
                if(ataReader.GetBlockSize())
                {
                    _dumpLog.WriteLine("ERROR: Cannot get block size: {0}.", ataReader.ErrorMessage);
                    ErrorMessage(ataReader.ErrorMessage);

                    return;
                }

                uint blockSize          = ataReader.LogicalBlockSize;
                uint physicalSectorSize = ataReader.PhysicalBlockSize;

                if(ataReader.FindReadCommand())
                {
                    _dumpLog.WriteLine("ERROR: Cannot find correct read command: {0}.", ataReader.ErrorMessage);
                    ErrorMessage(ataReader.ErrorMessage);

                    return;
                }

                // Check how many blocks to read, if error show and return
                if(ataReader.GetBlocksToRead(_maximumReadable))
                {
                    _dumpLog.WriteLine("ERROR: Cannot get blocks to read: {0}.", ataReader.ErrorMessage);
                    ErrorMessage(ataReader.ErrorMessage);

                    return;
                }

                uint   blocksToRead = ataReader.BlocksToRead;
                ushort cylinders    = ataReader.Cylinders;
                byte   heads        = ataReader.Heads;
                byte   sectors      = ataReader.Sectors;

                UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");

                UpdateStatus?.
                    Invoke($"Device reports {cylinders} cylinders {heads} heads {sectors} sectors per track.");

                UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time.");
                UpdateStatus?.Invoke($"Device reports {blockSize} bytes per logical block.");
                UpdateStatus?.Invoke($"Device reports {physicalSectorSize} bytes per physical block.");
                _dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).", blocks, blocks * blockSize);

                _dumpLog.WriteLine("Device reports {0} cylinders {1} heads {2} sectors per track.", cylinders, heads,
                                   sectors);

                _dumpLog.WriteLine("Device can read {0} blocks at a time.", blocksToRead);
                _dumpLog.WriteLine("Device reports {0} bytes per logical block.", blockSize);
                _dumpLog.WriteLine("Device reports {0} bytes per physical block.", physicalSectorSize);

                bool removable = !_dev.IsCompactFlash &&
                                 ataId.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.Removable);

                DumpHardwareType currentTry = null;
                ExtentsULong     extents    = null;

                ResumeSupport.Process(ataReader.IsLba, removable, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial,
                                      _dev.PlatformId, ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision,
                                      _private, _force);

                if(currentTry == null ||
                   extents    == null)
                {
                    StoppingErrorMessage?.Invoke("Could not process resume file, not continuing...");

                    return;
                }

                MhddLog mhddLog;
                IbgLog  ibgLog;
                double  duration;

                var ret = true;

                if(_dev.IsUsb                  &&
                   _dev.UsbDescriptors != null &&
                   !outputFormat.SupportedMediaTags.Contains(MediaTagType.USB_Descriptors))
                {
                    ret = false;
                    _dumpLog.WriteLine("Output format does not support USB descriptors.");
                    ErrorMessage("Output format does not support USB descriptors.");
                }

                if(_dev.IsPcmcia    &&
                   _dev.Cis != null &&
                   !outputFormat.SupportedMediaTags.Contains(MediaTagType.PCMCIA_CIS))
                {
                    ret = false;
                    _dumpLog.WriteLine("Output format does not support PCMCIA CIS descriptors.");
                    ErrorMessage("Output format does not support PCMCIA CIS descriptors.");
                }

                if(!outputFormat.SupportedMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
                {
                    ret = false;
                    _dumpLog.WriteLine("Output format does not support ATA IDENTIFY.");
                    ErrorMessage("Output format does not support ATA IDENTIFY.");
                }

                if(!ret)
                {
                    _dumpLog.WriteLine("Several media tags not supported, {0}continuing...", _force ? "" : "not ");

                    if(_force)
                        ErrorMessage("Several media tags not supported, continuing...");
                    else
                    {
                        StoppingErrorMessage?.Invoke("Several media tags not supported, not continuing...");

                        return;
                    }
                }

                mediaType = MediaTypeFromDevice.GetFromAta(_dev.Manufacturer, _dev.Model, _dev.IsRemovable,
                                                           _dev.IsCompactFlash, _dev.IsPcmcia, blocks);

                ret = outputFormat.Create(_outputPath, mediaType, _formatOptions, blocks, blockSize);

                // Cannot create image
                if(!ret)
                {
                    _dumpLog.WriteLine("Error creating output image, not continuing.");
                    _dumpLog.WriteLine(outputFormat.ErrorMessage);

                    StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                                 outputFormat.ErrorMessage);

                    return;
                }

                // Setting geometry
                outputFormat.SetGeometry(cylinders, heads, sectors);

                bool recoveredError;

                if(ataReader.IsLba)
                {
                    UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time.");

                    if(_skip < blocksToRead)
                        _skip = blocksToRead;

                    mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead,
                                          _private);

                    ibgLog = new IbgLog(_outputPrefix + ".ibg", ataProfile);

                    if(_resume.NextBlock > 0)
                    {
                        UpdateStatus?.Invoke($"Resuming from block {_resume.NextBlock}.");
                        _dumpLog.WriteLine("Resuming from block {0}.", _resume.NextBlock);
                    }

                    var newTrim = false;

                    start = DateTime.UtcNow;
                    DateTime timeSpeedStart   = DateTime.UtcNow;
                    ulong    sectorSpeedStart = 0;
                    InitProgress?.Invoke();

                    for(ulong i = _resume.NextBlock; i < blocks; i += blocksToRead)
                    {
                        if(_aborted)
                        {
                            currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                            UpdateStatus?.Invoke("Aborted!");
                            _dumpLog.WriteLine("Aborted!");

                            break;
                        }

                        if(blocks - i < blocksToRead)
                            blocksToRead = (byte)(blocks - i);

                        if(currentSpeed > maxSpeed &&
                           currentSpeed > 0)
                            maxSpeed = currentSpeed;

                        if(currentSpeed < minSpeed &&
                           currentSpeed > 0)
                            minSpeed = currentSpeed;

                        UpdateProgress?.Invoke($"Reading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)", (long)i,
                                               (long)blocks);

                        bool error = ataReader.ReadBlocks(out cmdBuf, i, blocksToRead, out duration, out _, out _);

                        if(!error)
                        {
                            mhddLog.Write(i, duration);
                            ibgLog.Write(i, currentSpeed * 1024);
                            DateTime writeStart = DateTime.Now;
                            outputFormat.WriteSectors(cmdBuf, i, blocksToRead);
                            imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                            extents.Add(i, blocksToRead, true);
                        }
                        else
                        {
                            if(i + _skip > blocks)
                                _skip = (uint)(blocks - i);

                            for(ulong b = i; b < i + _skip; b++)
                                _resume.BadBlocks.Add(b);

                            mhddLog.Write(i, duration < 500 ? 65535 : duration);

                            ibgLog.Write(i, 0);
                            DateTime writeStart = DateTime.Now;
                            outputFormat.WriteSectors(new byte[blockSize * _skip], i, _skip);
                            imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                            _dumpLog.WriteLine("Skipping {0} blocks from errored block {1}.", _skip, i);
                            i       += _skip - blocksToRead;
                            newTrim =  true;
                        }

                        sectorSpeedStart  += blocksToRead;
                        _resume.NextBlock =  i + blocksToRead;

                        double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                        if(elapsed <= 0)
                            continue;

                        currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                        sectorSpeedStart = 0;
                        timeSpeedStart   = DateTime.UtcNow;
                    }

                    _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

                    end = DateTime.Now;
                    EndProgress?.Invoke();
                    mhddLog.Close();

                    ibgLog.Close(_dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                 blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000), _devicePath);

                    UpdateStatus?.Invoke($"Dump finished in {(end - start).TotalSeconds} seconds.");

                    UpdateStatus?.
                        Invoke($"Average dump speed {blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000):F3} KiB/sec.");

                    UpdateStatus?.
                        Invoke($"Average write speed {blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration:F3} KiB/sec.");

                    _dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);

                    _dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                                       blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));

                    _dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                                       blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration);

                    #region Trimming
                    if(_resume.BadBlocks.Count > 0 &&
                       !_aborted                   &&
                       _trim                       &&
                       newTrim)
                    {
                        start = DateTime.UtcNow;
                        UpdateStatus?.Invoke("Trimming skipped sectors");
                        _dumpLog.WriteLine("Trimming skipped sectors");

                        ulong[] tmpArray = _resume.BadBlocks.ToArray();
                        InitProgress?.Invoke();

                        foreach(ulong badSector in tmpArray)
                        {
                            if(_aborted)
                            {
                                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                UpdateStatus?.Invoke("Aborted!");
                                _dumpLog.WriteLine("Aborted!");

                                break;
                            }

                            PulseProgress?.Invoke($"Trimming sector {badSector}");

                            bool error =
                                ataReader.ReadBlock(out cmdBuf, badSector, out duration, out recoveredError, out _);

                            totalDuration += duration;

                            if(error && !recoveredError)
                                continue;

                            _resume.BadBlocks.Remove(badSector);
                            extents.Add(badSector);
                            outputFormat.WriteSector(cmdBuf, badSector);
                        }

                        EndProgress?.Invoke();
                        end = DateTime.UtcNow;
                        UpdateStatus?.Invoke($"Trimming finished in {(end - start).TotalSeconds} seconds.");
                        _dumpLog.WriteLine("Trimming finished in {0} seconds.", (end - start).TotalSeconds);
                    }
                    #endregion Trimming

                    #region Error handling
                    if(_resume.BadBlocks.Count > 0 &&
                       !_aborted                   &&
                       _retryPasses > 0)
                    {
                        var pass    = 1;
                        var forward = true;

                        InitProgress?.Invoke();
                    repeatRetryLba:
                        ulong[] tmpArray = _resume.BadBlocks.ToArray();

                        foreach(ulong badSector in tmpArray)
                        {
                            if(_aborted)
                            {
                                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                UpdateStatus?.Invoke("Aborted!");
                                _dumpLog.WriteLine("Aborted!");

                                break;
                            }

                            PulseProgress?.Invoke(string.Format("Retrying sector {0}, pass {1}, {3}{2}", badSector,
                                                                pass, forward ? "forward" : "reverse",
                                                                _persistent ? "recovering partial data, " : ""));

                            bool error =
                                ataReader.ReadBlock(out cmdBuf, badSector, out duration, out recoveredError, out _);

                            totalDuration += duration;

                            if(!error || recoveredError)
                            {
                                _resume.BadBlocks.Remove(badSector);
                                extents.Add(badSector);
                                outputFormat.WriteSector(cmdBuf, badSector);
                                UpdateStatus?.Invoke($"Correctly retried block {badSector} in pass {pass}.");
                                _dumpLog.WriteLine("Correctly retried block {0} in pass {1}.", badSector, pass);
                            }
                            else if(_persistent)
                                outputFormat.WriteSector(cmdBuf, badSector);
                        }

                        if(pass < _retryPasses &&
                           !_aborted           &&
                           _resume.BadBlocks.Count > 0)
                        {
                            pass++;
                            forward = !forward;
                            _resume.BadBlocks.Sort();

                            if(!forward)
                                _resume.BadBlocks.Reverse();

                            goto repeatRetryLba;
                        }

                        EndProgress?.Invoke();
                    }
                    #endregion Error handling LBA

                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                }
                else
                {
                    mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead,
                                          _private);

                    ibgLog = new IbgLog(_outputPrefix + ".ibg", ataProfile);

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
                                if(_aborted)
                                {
                                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                    UpdateStatus?.Invoke("Aborted!");
                                    _dumpLog.WriteLine("Aborted!");

                                    break;
                                }

                                if(currentSpeed > maxSpeed &&
                                   currentSpeed > 0)
                                    maxSpeed = currentSpeed;

                                if(currentSpeed < minSpeed &&
                                   currentSpeed > 0)
                                    minSpeed = currentSpeed;

                                PulseProgress?.
                                    Invoke($"Reading cylinder {cy} head {hd} sector {sc} ({currentSpeed:F3} MiB/sec.)");

                                bool error =
                                    ataReader.ReadChs(out cmdBuf, cy, hd, sc, out duration, out recoveredError);

                                totalDuration += duration;

                                if(!error || recoveredError)
                                {
                                    mhddLog.Write(currentBlock, duration);
                                    ibgLog.Write(currentBlock, currentSpeed * 1024);
                                    DateTime writeStart = DateTime.Now;

                                    outputFormat.WriteSector(cmdBuf, (ulong)((cy * heads + hd) * sectors + (sc - 1)));

                                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                                    extents.Add(currentBlock);

                                    _dumpLog.WriteLine("Error reading cylinder {0} head {1} sector {2}.", cy, hd, sc);
                                }
                                else
                                {
                                    _resume.BadBlocks.Add(currentBlock);
                                    mhddLog.Write(currentBlock, duration < 500 ? 65535 : duration);

                                    ibgLog.Write(currentBlock, 0);
                                    DateTime writeStart = DateTime.Now;

                                    outputFormat.WriteSector(new byte[blockSize],
                                                             (ulong)((cy * heads + hd) * sectors + (sc - 1)));

                                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                                }

                                sectorSpeedStart++;
                                currentBlock++;

                                double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                                if(elapsed <= 0)
                                    continue;

                                currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                                sectorSpeedStart = 0;
                                timeSpeedStart   = DateTime.UtcNow;
                            }
                        }
                    }

                    _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

                    end = DateTime.Now;
                    EndProgress?.Invoke();
                    mhddLog.Close();

                    ibgLog.Close(_dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                 blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000), _devicePath);

                    UpdateStatus?.Invoke($"Dump finished in {(end - start).TotalSeconds} seconds.");

                    UpdateStatus?.
                        Invoke($"Average dump speed {blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000):F3} KiB/sec.");

                    UpdateStatus?.
                        Invoke($"Average write speed {blockSize * (double)(blocks + 1) / 1024 / (imageWriteDuration / 1000):F3} KiB/sec.");

                    _dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);

                    _dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                                       blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));

                    _dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                                       blockSize * (double)(blocks + 1) / 1024 / (imageWriteDuration / 1000));
                }

                foreach(ulong bad in _resume.BadBlocks)
                    _dumpLog.WriteLine("Sector {0} could not be read.", bad);

                outputFormat.SetDumpHardware(_resume.Tries);

                // TODO: Non-removable
                var metadata = new ImageInfo
                {
                    Application        = "Aaru",
                    ApplicationVersion = Version.GetVersion()
                };

                if(!outputFormat.SetMetadata(metadata))
                    ErrorMessage?.Invoke("Error {0} setting metadata, continuing..." + Environment.NewLine +
                                         outputFormat.ErrorMessage);

                if(_preSidecar != null)
                    outputFormat.SetCicmMetadata(_preSidecar);

                _dumpLog.WriteLine("Closing output file.");
                UpdateStatus?.Invoke("Closing output file.");
                DateTime closeStart = DateTime.Now;
                outputFormat.Close();
                DateTime closeEnd = DateTime.Now;
                UpdateStatus?.Invoke($"Closed in {(closeEnd - closeStart).TotalSeconds} seconds.");
                _dumpLog.WriteLine("Closed in {0} seconds.", (closeEnd - closeStart).TotalSeconds);

                if(_aborted)
                {
                    _dumpLog.WriteLine("Aborted!");
                    UpdateStatus?.Invoke("Aborted!");

                    return;
                }

                double totalChkDuration = 0;

                outputFormat.WriteMediaTag(ataIdentify, MediaTagType.ATA_IDENTIFY);

                if(_dev.IsUsb &&
                   _dev.UsbDescriptors != null)
                    outputFormat.WriteMediaTag(_dev.UsbDescriptors, MediaTagType.USB_Descriptors);

                if(_dev.IsPcmcia &&
                   _dev.Cis != null)
                    outputFormat.WriteMediaTag(_dev.Cis, MediaTagType.PCMCIA_CIS);

                if(_metadata)
                {
                    _dumpLog.WriteLine("Creating sidecar.");
                    UpdateStatus?.Invoke("Creating sidecar.");
                    var         filters     = new FiltersList();
                    IFilter     filter      = filters.GetFilter(_outputPath);
                    var         inputPlugin = ImageFormat.Detect(filter) as IMediaImage;
                    ErrorNumber opened      = inputPlugin.Open(filter);

                    if(opened != ErrorNumber.NoError)
                    {
                        StoppingErrorMessage?.Invoke($"Error {opened} opening created image.");

                        return;
                    }

                    DateTime chkStart = DateTime.UtcNow;

                    _sidecarClass = new Sidecar(inputPlugin, _outputPath, filter.Id, _encoding);

                    _sidecarClass.InitProgressEvent    += InitProgress;
                    _sidecarClass.UpdateProgressEvent  += UpdateProgress;
                    _sidecarClass.EndProgressEvent     += EndProgress;
                    _sidecarClass.InitProgressEvent2   += InitProgress2;
                    _sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
                    _sidecarClass.EndProgressEvent2    += EndProgress2;
                    _sidecarClass.UpdateStatusEvent    += UpdateStatus;
                    CICMMetadataType sidecar = _sidecarClass.Create();

                    if(!_aborted)
                    {
                        if(_preSidecar != null)
                        {
                            _preSidecar.BlockMedia = sidecar.BlockMedia;
                            sidecar                = _preSidecar;
                        }

                        if(_dev.IsUsb &&
                           _dev.UsbDescriptors != null)
                        {
                            _dumpLog.WriteLine("Reading USB descriptors.");
                            UpdateStatus?.Invoke("Reading USB descriptors.");

                            sidecar.BlockMedia[0].USB = new USBType
                            {
                                ProductID = _dev.UsbProductId,
                                VendorID  = _dev.UsbVendorId,
                                Descriptors = new DumpType
                                {
                                    Image     = _outputPath,
                                    Size      = (ulong)_dev.UsbDescriptors.Length,
                                    Checksums = Checksum.GetChecksums(_dev.UsbDescriptors).ToArray()
                                }
                            };
                        }

                        if(_dev.IsPcmcia &&
                           _dev.Cis != null)
                        {
                            _dumpLog.WriteLine("Reading PCMCIA CIS.");
                            UpdateStatus?.Invoke("Reading PCMCIA CIS.");

                            sidecar.BlockMedia[0].PCMCIA = new PCMCIAType
                            {
                                CIS = new DumpType
                                {
                                    Image     = _outputPath,
                                    Size      = (ulong)_dev.Cis.Length,
                                    Checksums = Checksum.GetChecksums(_dev.Cis).ToArray()
                                }
                            };

                            _dumpLog.WriteLine("Decoding PCMCIA CIS.");
                            UpdateStatus?.Invoke("Decoding PCMCIA CIS.");
                            Tuple[] tuples = CIS.GetTuples(_dev.Cis);

                            if(tuples != null)
                                foreach(Tuple tuple in tuples)
                                    switch(tuple.Code)
                                    {
                                        case TupleCodes.CISTPL_MANFID:
                                            ManufacturerIdentificationTuple manufacturerId =
                                                CIS.DecodeManufacturerIdentificationTuple(tuple);

                                            if(manufacturerId != null)
                                            {
                                                sidecar.BlockMedia[0].PCMCIA.ManufacturerCode =
                                                    manufacturerId.ManufacturerID;

                                                sidecar.BlockMedia[0].PCMCIA.CardCode = manufacturerId.CardID;
                                                sidecar.BlockMedia[0].PCMCIA.ManufacturerCodeSpecified = true;
                                                sidecar.BlockMedia[0].PCMCIA.CardCodeSpecified = true;
                                            }

                                            break;
                                        case TupleCodes.CISTPL_VERS_1:
                                            Level1VersionTuple version = CIS.DecodeLevel1VersionTuple(tuple);

                                            if(version != null)
                                            {
                                                sidecar.BlockMedia[0].PCMCIA.Manufacturer = version.Manufacturer;
                                                sidecar.BlockMedia[0].PCMCIA.ProductName  = version.Product;

                                                sidecar.BlockMedia[0].PCMCIA.Compliance =
                                                    $"{version.MajorVersion}.{version.MinorVersion}";

                                                sidecar.BlockMedia[0].PCMCIA.AdditionalInformation =
                                                    version.AdditionalInformation;
                                            }

                                            break;
                                    }
                        }

                        if(_private)
                            DeviceReport.ClearIdentify(ataIdentify);

                        sidecar.BlockMedia[0].ATA = new ATAType
                        {
                            Identify = new DumpType
                            {
                                Image     = _outputPath,
                                Size      = (ulong)cmdBuf.Length,
                                Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                            }
                        };

                        DateTime chkEnd = DateTime.UtcNow;

                        totalChkDuration = (chkEnd - chkStart).TotalMilliseconds;
                        UpdateStatus?.Invoke($"Sidecar created in {(chkEnd - chkStart).TotalSeconds} seconds.");

                        UpdateStatus?.
                            Invoke($"Average checksum speed {blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000):F3} KiB/sec.");

                        _dumpLog.WriteLine("Sidecar created in {0} seconds.", (chkEnd - chkStart).TotalSeconds);

                        _dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                                           blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                        List<(ulong start, string type)> filesystems = new();

                        if(sidecar.BlockMedia[0].FileSystemInformation != null)
                            filesystems.AddRange(from partition in sidecar.BlockMedia[0].FileSystemInformation
                                                 where partition.FileSystems != null
                                                 from fileSystem in partition.FileSystems
                                                 select (partition.StartSector, fileSystem.Type));

                        if(filesystems.Count > 0)
                            foreach(var filesystem in filesystems.Select(o => new
                                    {
                                        o.start,
                                        o.type
                                    }).Distinct())
                            {
                                UpdateStatus?.
                                    Invoke($"Found filesystem {filesystem.type} at sector {filesystem.start}");

                                _dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type,
                                                   filesystem.start);
                            }

                        (string type, string subType) = CommonTypes.Metadata.MediaType.MediaTypeToString(mediaType);

                        sidecar.BlockMedia[0].DiskType          = type;
                        sidecar.BlockMedia[0].DiskSubType       = subType;
                        sidecar.BlockMedia[0].Interface         = "ATA";
                        sidecar.BlockMedia[0].LogicalBlocks     = blocks;
                        sidecar.BlockMedia[0].PhysicalBlockSize = physicalSectorSize;
                        sidecar.BlockMedia[0].LogicalBlockSize  = blockSize;
                        sidecar.BlockMedia[0].Manufacturer      = _dev.Manufacturer;
                        sidecar.BlockMedia[0].Model             = _dev.Model;

                        if(!_private)
                            sidecar.BlockMedia[0].Serial = _dev.Serial;

                        sidecar.BlockMedia[0].Size = blocks * blockSize;

                        if(cylinders > 0 &&
                           heads     > 0 &&
                           sectors   > 0)
                        {
                            sidecar.BlockMedia[0].Cylinders                = cylinders;
                            sidecar.BlockMedia[0].CylindersSpecified       = true;
                            sidecar.BlockMedia[0].Heads                    = heads;
                            sidecar.BlockMedia[0].HeadsSpecified           = true;
                            sidecar.BlockMedia[0].SectorsPerTrack          = sectors;
                            sidecar.BlockMedia[0].SectorsPerTrackSpecified = true;
                        }

                        UpdateStatus?.Invoke("Writing metadata sidecar");

                        var xmlFs = new FileStream(_outputPrefix + ".cicm.xml", FileMode.Create);

                        var xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                        xmlSer.Serialize(xmlFs, sidecar);
                        xmlFs.Close();
                    }
                }

                UpdateStatus?.Invoke("");

                UpdateStatus?.
                    Invoke($"Took a total of {(end - start).TotalSeconds:F3} seconds ({totalDuration / 1000:F3} processing commands, {totalChkDuration / 1000:F3} checksumming, {imageWriteDuration:F3} writing, {(closeEnd - closeStart).TotalSeconds:F3} closing).");

                UpdateStatus?.
                    Invoke($"Average speed: {blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000):F3} MiB/sec.");

                if(maxSpeed > 0)
                    UpdateStatus?.Invoke($"Fastest speed burst: {maxSpeed:F3} MiB/sec.");

                if(minSpeed is > 0 and < double.MaxValue)
                    UpdateStatus?.Invoke($"Slowest speed burst: {minSpeed:F3} MiB/sec.");

                UpdateStatus?.Invoke($"{_resume.BadBlocks.Count} sectors could not be read.");

                if(_resume.BadBlocks.Count > 0)
                    _resume.BadBlocks.Sort();

                UpdateStatus?.Invoke("");
            }

            Statistics.AddMedia(mediaType, true);
        }
        else
            StoppingErrorMessage?.Invoke("Unable to communicate with ATA device.");
    }
}