// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Core.Logging;
using Aaru.Decoders.MMC;
using Aaru.Decoders.SecureDigital;
using Schemas;
using CSD = Aaru.Decoders.MMC.CSD;
using DeviceType = Aaru.CommonTypes.Enums.DeviceType;
using MediaType = Aaru.CommonTypes.MediaType;
using Version = Aaru.CommonTypes.Interop.Version;

// ReSharper disable JoinDeclarationAndInitializer

namespace Aaru.Core.Devices.Dumping
{
    /// <summary>Implements dumping a MultiMediaCard or SecureDigital flash card</summary>
    public partial class Dump
    {
        /// <summary>Dumps a MultiMediaCard or SecureDigital flash card</summary>
        void SecureDigital()
        {
            if(_dumpRaw)
            {
                if(_force)
                    ErrorMessage?.
                        Invoke("Raw dumping is not supported in MultiMediaCard or SecureDigital devices. Continuing...");
                else
                {
                    StoppingErrorMessage?.
                        Invoke("Raw dumping is not supported in MultiMediaCard or SecureDigital devices. Aborting...");

                    return;
                }
            }

            bool         sense;
            const ushort sdProfile = 0x0001;
            const uint   timeout   = 5;
            double       duration;
            ushort       blocksToRead      = 128;
            uint         blockSize         = 512;
            ulong        blocks            = 0;
            byte[]       csd               = null;
            byte[]       ocr               = null;
            byte[]       ecsd              = null;
            byte[]       scr               = null;
            uint         physicalBlockSize = 0;
            bool         byteAddressed     = true;
            uint[]       response;
            bool         supportsCmd23 = false;

            Dictionary<MediaTagType, byte[]> mediaTags = new();

            switch(_dev.Type)
            {
                case DeviceType.MMC:
                {
                    UpdateStatus?.Invoke("Reading CSD");
                    _dumpLog.WriteLine("Reading CSD");
                    sense = _dev.ReadCsd(out csd, out response, timeout, out duration);

                    if(!sense)
                    {
                        CSD csdDecoded = Decoders.MMC.Decoders.DecodeCSD(csd);
                        blocks    = (ulong)((csdDecoded.Size + 1) * Math.Pow(2, csdDecoded.SizeMultiplier + 2));
                        blockSize = (uint)Math.Pow(2, csdDecoded.ReadBlockLength);

                        mediaTags.Add(MediaTagType.MMC_CSD, null);

                        // Found at least since MMC System Specification 3.31
                        supportsCmd23 = csdDecoded.Version >= 3;

                        if(csdDecoded.Size == 0xFFF)
                        {
                            UpdateStatus?.Invoke("Reading Extended CSD");
                            _dumpLog.WriteLine("Reading Extended CSD");
                            sense = _dev.ReadExtendedCsd(out ecsd, out response, timeout, out duration);

                            if(!sense)
                            {
                                ExtendedCSD ecsdDecoded = Decoders.MMC.Decoders.DecodeExtendedCSD(ecsd);
                                blocks    = ecsdDecoded.SectorCount;
                                blockSize = (uint)(ecsdDecoded.SectorSize == 1 ? 4096 : 512);

                                if(ecsdDecoded.NativeSectorSize == 0)
                                    physicalBlockSize = 512;
                                else if(ecsdDecoded.NativeSectorSize == 1)
                                    physicalBlockSize = 4096;

                                blocksToRead = (ushort)(ecsdDecoded.OptimalReadSize * 4096 / blockSize);

                                if(blocksToRead == 0)
                                    blocksToRead = 128;

                                // Supposing it's high-capacity MMC if it has Extended CSD...
                                byteAddressed = false;
                                mediaTags.Add(MediaTagType.MMC_ExtendedCSD, null);
                            }
                            else
                            {
                                _errorLog?.WriteLine("Read eCSD", _dev.Error, _dev.LastError, response);
                                ecsd = null;
                            }
                        }
                    }
                    else
                    {
                        _errorLog?.WriteLine("Read CSD", _dev.Error, _dev.LastError, response);
                        csd = null;
                    }

                    UpdateStatus?.Invoke("Reading OCR");
                    _dumpLog.WriteLine("Reading OCR");
                    sense = _dev.ReadOcr(out ocr, out response, timeout, out duration);

                    if(sense)
                    {
                        _errorLog?.WriteLine("Read OCR", _dev.Error, _dev.LastError, response);
                        ocr = null;
                    }
                    else
                        mediaTags.Add(MediaTagType.MMC_OCR, null);

                    break;
                }

                case DeviceType.SecureDigital:
                {
                    UpdateStatus?.Invoke("Reading CSD");
                    _dumpLog.WriteLine("Reading CSD");
                    sense = _dev.ReadCsd(out csd, out response, timeout, out duration);

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

                        physicalBlockSize = blockSize;

                        if(blockSize != 512)
                        {
                            uint ratio = blockSize / 512;
                            blocks    *= ratio;
                            blockSize =  512;
                        }
                    }
                    else
                    {
                        _errorLog?.WriteLine("Read CSD", _dev.Error, _dev.LastError, response);
                        csd = null;
                    }

                    UpdateStatus?.Invoke("Reading OCR");
                    _dumpLog.WriteLine("Reading OCR");
                    sense = _dev.ReadSdocr(out ocr, out response, timeout, out duration);

                    if(sense)
                    {
                        _errorLog?.WriteLine("Read OCR", _dev.Error, _dev.LastError, response);
                        ocr = null;
                    }
                    else
                        mediaTags.Add(MediaTagType.SD_OCR, null);

                    UpdateStatus?.Invoke("Reading SCR");
                    _dumpLog.WriteLine("Reading SCR");
                    sense = _dev.ReadScr(out scr, out response, timeout, out duration);

                    if(sense)
                    {
                        _errorLog?.WriteLine("Read SCR", _dev.Error, _dev.LastError, response);
                        scr = null;
                    }
                    else
                    {
                        supportsCmd23 = Decoders.SecureDigital.Decoders.DecodeSCR(scr)?.CommandSupport.
                                                 HasFlag(CommandSupport.SetBlockCount) ?? false;

                        mediaTags.Add(MediaTagType.SD_SCR, null);
                    }

                    break;
                }
            }

            UpdateStatus?.Invoke("Reading CID");
            _dumpLog.WriteLine("Reading CID");
            sense = _dev.ReadCid(out byte[] cid, out response, timeout, out duration);

            if(sense)
            {
                _errorLog?.WriteLine("Read CID", _dev.Error, _dev.LastError, response);
                cid = null;
            }
            else
                mediaTags.Add(_dev.Type == DeviceType.SecureDigital ? MediaTagType.SD_CID : MediaTagType.MMC_CID, null);

            DateTime start;
            DateTime end;
            double   totalDuration = 0;
            double   currentSpeed  = 0;
            double   maxSpeed      = double.MinValue;
            double   minSpeed      = double.MaxValue;

            if(blocks == 0)
            {
                _dumpLog.WriteLine("Unable to get device size.");
                StoppingErrorMessage?.Invoke("Unable to get device size.");

                return;
            }

            UpdateStatus?.Invoke($"Device reports {blocks} blocks.");
            _dumpLog.WriteLine("Device reports {0} blocks.", blocks);

            byte[] cmdBuf;
            bool   error;

            if(blocksToRead > _maximumReadable)
                blocksToRead = (ushort)_maximumReadable;

            if(supportsCmd23 && blocksToRead > 1)
            {
                sense = _dev.ReadWithBlockCount(out cmdBuf, out _, 0, blockSize, 1, byteAddressed, timeout,
                                                out duration);

                if(sense || _dev.Error)
                    supportsCmd23 = false;

                // Need to restart device, otherwise is it just busy streaming data with no one listening
                sense = _dev.ReOpen();

                if(sense)
                {
                    StoppingErrorMessage?.Invoke($"Error {_dev.LastError} reopening device.");

                    return;
                }
            }

            if(supportsCmd23 && blocksToRead > 1)
            {
                while(true)
                {
                    error = _dev.ReadWithBlockCount(out cmdBuf, out _, 0, blockSize, blocksToRead, byteAddressed,
                                                    timeout, out duration);

                    if(error)
                        blocksToRead /= 2;

                    if(!error ||
                       blocksToRead == 1)
                        break;
                }

                if(error)
                {
                    _dumpLog.WriteLine("ERROR: Cannot get blocks to read, device error {0}.", _dev.LastError);

                    StoppingErrorMessage?.
                        Invoke($"Device error {_dev.LastError} trying to guess ideal transfer length.");

                    return;
                }
            }

            if(supportsCmd23 || blocksToRead == 1)
            {
                UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time.");
                _dumpLog.WriteLine("Device can read {0} blocks at a time.", blocksToRead);
            }
            else if(_useBufferedReads)
            {
                UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time using OS buffered reads.");
                _dumpLog.WriteLine("Device can read {0} blocks at a time using OS buffered reads.", blocksToRead);
            }
            else
            {
                UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks using sequential commands.");
                _dumpLog.WriteLine("Device can read {0} blocks using sequential commands.", blocksToRead);
            }

            if(_skip < blocksToRead)
                _skip = blocksToRead;

            DumpHardwareType currentTry = null;
            ExtentsULong     extents    = null;

            ResumeSupport.Process(true, false, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial, _dev.PlatformId,
                                  ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision, _private, _force);

            if(currentTry == null ||
               extents    == null)
            {
                StoppingErrorMessage?.Invoke("Could not process resume file, not continuing...");

                return;
            }

            bool ret = true;

            foreach(MediaTagType tag in mediaTags.Keys.Where(tag => !_outputPlugin.SupportedMediaTags.Contains(tag)))
            {
                ret = false;
                _dumpLog.WriteLine($"Output format does not support {tag}.");
                ErrorMessage?.Invoke($"Output format does not support {tag}.");
            }

            if(!ret)
            {
                if(_force)
                {
                    _dumpLog.WriteLine("Several media tags not supported, continuing...");
                    ErrorMessage?.Invoke("Several media tags not supported, continuing...");
                }
                else
                {
                    _dumpLog.WriteLine("Several media tags not supported, not continuing...");
                    StoppingErrorMessage?.Invoke("Several media tags not supported, not continuing...");

                    return;
                }
            }

            var mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead, _private);
            var ibgLog  = new IbgLog(_outputPrefix  + ".ibg", sdProfile);

            ret = _outputPlugin.Create(_outputPath,
                                       _dev.Type == DeviceType.SecureDigital ? MediaType.SecureDigital : MediaType.MMC,
                                       _formatOptions, blocks, blockSize);

            // Cannot create image
            if(!ret)
            {
                _dumpLog.WriteLine("Error creating output image, not continuing.");
                _dumpLog.WriteLine(_outputPlugin.ErrorMessage);

                StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                             _outputPlugin.ErrorMessage);

                return;
            }

            if(cid != null)
            {
                if(_dev.Type == DeviceType.SecureDigital && _private)
                {
                    // Clear serial number and manufacturing date
                    cid[9]  = 0;
                    cid[10] = 0;
                    cid[11] = 0;
                    cid[12] = 0;
                    cid[13] = 0;
                    cid[14] = 0;
                }
                else if(_dev.Type == DeviceType.MMC && _private)
                {
                    // Clear serial number and manufacturing date
                    cid[10] = 0;
                    cid[11] = 0;
                    cid[12] = 0;
                    cid[13] = 0;
                    cid[14] = 0;
                }

                ret =
                    _outputPlugin.WriteMediaTag(cid,
                                                _dev.Type == DeviceType.SecureDigital ? MediaTagType.SD_CID
                                                    : MediaTagType.MMC_CID);

                // Cannot write CID to image
                if(!ret &&
                   !_force)
                {
                    _dumpLog.WriteLine("Cannot write CID to output image.");

                    StoppingErrorMessage?.Invoke("Cannot write CID to output image." + Environment.NewLine +
                                                 _outputPlugin.ErrorMessage);

                    return;
                }
            }

            if(csd != null)
            {
                ret =
                    _outputPlugin.WriteMediaTag(csd,
                                                _dev.Type == DeviceType.SecureDigital ? MediaTagType.SD_CSD
                                                    : MediaTagType.MMC_CSD);

                // Cannot write CSD to image
                if(!ret &&
                   !_force)
                {
                    _dumpLog.WriteLine("Cannot write CSD to output image.");

                    StoppingErrorMessage?.Invoke("Cannot write CSD to output image." + Environment.NewLine +
                                                 _outputPlugin.ErrorMessage);

                    return;
                }
            }

            if(ecsd != null)
            {
                ret = _outputPlugin.WriteMediaTag(ecsd, MediaTagType.MMC_ExtendedCSD);

                // Cannot write Extended CSD to image
                if(!ret &&
                   !_force)
                {
                    _dumpLog.WriteLine("Cannot write Extended CSD to output image.");

                    StoppingErrorMessage?.Invoke("Cannot write Extended CSD to output image." + Environment.NewLine +
                                                 _outputPlugin.ErrorMessage);

                    return;
                }
            }

            if(ocr != null)
            {
                ret =
                    _outputPlugin.WriteMediaTag(ocr,
                                                _dev.Type == DeviceType.SecureDigital ? MediaTagType.SD_OCR
                                                    : MediaTagType.MMC_OCR);

                // Cannot write OCR to image
                if(!ret &&
                   !_force)
                {
                    _dumpLog.WriteLine("Cannot write OCR to output image.");

                    StoppingErrorMessage?.Invoke("Cannot write OCR to output image." + Environment.NewLine +
                                                 _outputPlugin.ErrorMessage);

                    return;
                }
            }

            if(scr != null)
            {
                ret = _outputPlugin.WriteMediaTag(scr, MediaTagType.SD_SCR);

                // Cannot write SCR to image
                if(!ret &&
                   !_force)
                {
                    _dumpLog.WriteLine("Cannot write SCR to output image.");

                    StoppingErrorMessage?.Invoke("Cannot write SCR to output image." + Environment.NewLine +
                                                 _outputPlugin.ErrorMessage);

                    return;
                }
            }

            if(_resume.NextBlock > 0)
            {
                UpdateStatus?.Invoke($"Resuming from block {_resume.NextBlock}.");
                _dumpLog.WriteLine("Resuming from block {0}.", _resume.NextBlock);
            }

            start = DateTime.UtcNow;
            double   imageWriteDuration = 0;
            bool     newTrim            = false;
            DateTime timeSpeedStart     = DateTime.UtcNow;
            ulong    sectorSpeedStart   = 0;

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

                if(blocksToRead == 1)
                    error = _dev.ReadSingleBlock(out cmdBuf, out _, (uint)i, blockSize, byteAddressed, timeout,
                                                 out duration);
                else if(supportsCmd23)
                    error = _dev.ReadWithBlockCount(out cmdBuf, out _, (uint)i, blockSize, blocksToRead, byteAddressed,
                                                    timeout, out duration);
                else if(_useBufferedReads)
                    error = _dev.BufferedOsRead(out cmdBuf, (long)(i * blockSize), blockSize * blocksToRead,
                                                out duration);
                else
                    error = _dev.ReadMultipleUsingSingle(out cmdBuf, out _, (uint)i, blockSize, blocksToRead,
                                                         byteAddressed, timeout, out duration);

                if(!error)
                {
                    mhddLog.Write(i, duration);
                    ibgLog.Write(i, currentSpeed * 1024);
                    DateTime writeStart = DateTime.Now;
                    _outputPlugin.WriteSectors(cmdBuf, i, blocksToRead);
                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                    extents.Add(i, blocksToRead, true);
                }
                else
                {
                    _errorLog?.WriteLine(i, _dev.Error, _dev.LastError, byteAddressed, response);

                    if(i + _skip > blocks)
                        _skip = (uint)(blocks - i);

                    for(ulong b = i; b < i + _skip; b++)
                        _resume.BadBlocks.Add(b);

                    mhddLog.Write(i, duration < 500 ? 65535 : duration);

                    ibgLog.Write(i, 0);
                    DateTime writeStart = DateTime.Now;
                    _outputPlugin.WriteSectors(new byte[blockSize * _skip], i, _skip);
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

                    error = _dev.ReadSingleBlock(out cmdBuf, out response, (uint)badSector, blockSize, byteAddressed,
                                                 timeout, out duration);

                    totalDuration += duration;

                    if(error)
                    {
                        _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, byteAddressed, response);

                        continue;
                    }

                    _resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                    _outputPlugin.WriteSector(cmdBuf, badSector);
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
                int  pass              = 1;
                bool forward           = true;
                bool runningPersistent = false;

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

                    PulseProgress?.Invoke(string.Format("Retrying sector {0}, pass {1}, {3}{2}", badSector, pass,
                                                        forward ? "forward" : "reverse",
                                                        runningPersistent ? "recovering partial data, " : ""));

                    error = _dev.ReadSingleBlock(out cmdBuf, out response, (uint)badSector, blockSize, byteAddressed,
                                                 timeout, out duration);

                    totalDuration += duration;

                    if(error)
                        _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, byteAddressed, response);

                    if(!error)
                    {
                        _resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        _outputPlugin.WriteSector(cmdBuf, badSector);
                        UpdateStatus?.Invoke($"Correctly retried block {badSector} in pass {pass}.");
                        _dumpLog.WriteLine("Correctly retried block {0} in pass {1}.", badSector, pass);
                    }
                    else if(runningPersistent)
                        _outputPlugin.WriteSector(cmdBuf, badSector);
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
            #endregion Error handling

            currentTry.Extents = ExtentsConverter.ToMetadata(extents);

            _outputPlugin.SetDumpHardware(_resume.Tries);

            // TODO: Drive info
            var metadata = new CommonTypes.Structs.ImageInfo
            {
                Application        = "Aaru",
                ApplicationVersion = Version.GetVersion()
            };

            if(!_outputPlugin.SetMetadata(metadata))
                ErrorMessage?.Invoke("Error {0} setting metadata, continuing..." + Environment.NewLine +
                                     _outputPlugin.ErrorMessage);

            if(_preSidecar != null)
                _outputPlugin.SetCicmMetadata(_preSidecar);

            _dumpLog.WriteLine("Closing output file.");
            UpdateStatus?.Invoke("Closing output file.");
            DateTime closeStart = DateTime.Now;
            _outputPlugin.Close();
            DateTime closeEnd = DateTime.Now;
            UpdateStatus?.Invoke($"Closed in {(closeEnd - closeStart).TotalSeconds} seconds.");
            _dumpLog.WriteLine("Closed in {0} seconds.", (closeEnd - closeStart).TotalSeconds);

            if(_aborted)
            {
                UpdateStatus?.Invoke("Aborted!");
                _dumpLog.WriteLine("Aborted!");

                return;
            }

            double totalChkDuration = 0;

            if(_metadata)
            {
                UpdateStatus?.Invoke("Creating sidecar.");
                _dumpLog.WriteLine("Creating sidecar.");
                var         filters     = new FiltersList();
                IFilter     filter      = filters.GetFilter(_outputPath);
                IMediaImage inputPlugin = ImageFormat.Detect(filter) as IMediaImage;
                ErrorNumber opened      = inputPlugin.Open(filter);

                if(opened != ErrorNumber.NoError)
                    StoppingErrorMessage?.Invoke($"Error {opened} opening created image.");

                DateTime chkStart = DateTime.UtcNow;
                _sidecarClass                      =  new Sidecar(inputPlugin, _outputPath, filter.Id, _encoding);
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

                    end = DateTime.UtcNow;

                    totalChkDuration = (end - chkStart).TotalMilliseconds;
                    UpdateStatus?.Invoke($"Sidecar created in {(end - chkStart).TotalSeconds} seconds.");

                    UpdateStatus?.
                        Invoke($"Average checksum speed {blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000):F3} KiB/sec.");

                    _dumpLog.WriteLine("Sidecar created in {0} seconds.", (end - chkStart).TotalSeconds);

                    _dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                                       blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                    (string type, string subType) xmlType = (null, null);

                    switch(_dev.Type)
                    {
                        case DeviceType.MMC:
                            xmlType = CommonTypes.Metadata.MediaType.MediaTypeToString(MediaType.MMC);

                            sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(MediaType.MMC);

                            break;
                        case DeviceType.SecureDigital:
                            CommonTypes.Metadata.MediaType.MediaTypeToString(MediaType.SecureDigital);

                            sidecar.BlockMedia[0].Dimensions =
                                Dimensions.DimensionsFromMediaType(MediaType.SecureDigital);

                            break;
                    }

                    sidecar.BlockMedia[0].DiskType    = xmlType.type;
                    sidecar.BlockMedia[0].DiskSubType = xmlType.subType;

                    // TODO: Implement device firmware revision
                    sidecar.BlockMedia[0].LogicalBlocks     = blocks;
                    sidecar.BlockMedia[0].PhysicalBlockSize = physicalBlockSize > 0 ? physicalBlockSize : blockSize;
                    sidecar.BlockMedia[0].LogicalBlockSize  = blockSize;
                    sidecar.BlockMedia[0].Manufacturer      = _dev.Manufacturer;
                    sidecar.BlockMedia[0].Model             = _dev.Model;

                    if(!_private)
                        sidecar.BlockMedia[0].Serial = _dev.Serial;

                    sidecar.BlockMedia[0].Size = blocks * blockSize;

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

            if(minSpeed > 0 &&
               minSpeed < double.MaxValue)
                UpdateStatus?.Invoke($"Slowest speed burst: {minSpeed:F3} MiB/sec.");

            UpdateStatus?.Invoke($"{_resume.BadBlocks.Count} sectors could not be read.");
            UpdateStatus?.Invoke("");

            if(_resume.BadBlocks.Count > 0)
                _resume.BadBlocks.Sort();

            switch(_dev.Type)
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