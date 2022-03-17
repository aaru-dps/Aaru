// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MiniDisc.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps MiniDisc devices.
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



// ReSharper disable JoinDeclarationAndInitializer

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
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core.Logging;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Schemas;
using MediaType = Aaru.CommonTypes.MediaType;
using Version = Aaru.CommonTypes.Interop.Version;

/// <summary>Implements dumping MiniDisc Data devices</summary>
partial class Dump
{
    /// <summary>Dumps a MiniDisc Data device</summary>
    void MiniDisc()
    {
        bool                             sense;
        byte                             scsiMediumType = 0;
        const ushort                     sbcProfile     = 0x0001;
        DateTime                         start;
        DateTime                         end;
        double                           totalDuration = 0;
        double                           currentSpeed  = 0;
        double                           maxSpeed      = double.MinValue;
        double                           minSpeed      = double.MaxValue;
        byte[]                           readBuffer;
        Modes.DecodedMode?               decMode   = null;
        Dictionary<MediaTagType, byte[]> mediaTags = new();
        byte[]                           cmdBuf;
        bool                             ret;

        if(_outputPlugin is not IWritableImage outputFormat)
        {
            StoppingErrorMessage?.Invoke("Image is not writable, aborting...");

            return;
        }

        _dumpLog.WriteLine("Initializing reader.");
        var   scsiReader = new Reader(_dev, _dev.Timeout, null, _errorLog);
        ulong blocks     = scsiReader.GetDeviceBlocks();
        uint  blockSize  = scsiReader.LogicalBlockSize;

        _dumpLog.WriteLine("Requesting MODE SENSE (6).");
        UpdateStatus?.Invoke("Requesting MODE SENSE (6).");

        sense = _dev.ModeSense6(out cmdBuf, out _, true, ScsiModeSensePageControl.Current, 0x3F, 5, out _);

        if(!sense      &&
           !_dev.Error &&
           Modes.DecodeMode6(cmdBuf, _dev.ScsiType).HasValue)
            decMode = Modes.DecodeMode6(cmdBuf, _dev.ScsiType);

        if(decMode.HasValue)
            scsiMediumType = (byte)decMode.Value.Header.MediumType;

        if(blockSize != 2048)
        {
            _dumpLog.WriteLine("MiniDisc albums, NetMD discs or user-written audio MiniDisc cannot be dumped.");

            StoppingErrorMessage?.
                Invoke("MiniDisc albums, NetMD discs or user-written audio MiniDisc cannot be dumped.");

            return;
        }

        MediaType dskType = MediaType.MDData;

        if(scsiReader.FindReadCommand())
        {
            _dumpLog.WriteLine("ERROR: Cannot find correct read command: {0}.", scsiReader.ErrorMessage);
            StoppingErrorMessage?.Invoke("Unable to read medium.");

            return;
        }

        if(blocks    != 0 &&
           blockSize != 0)
        {
            blocks++;

            ulong totalSize = blocks * blockSize;

            if(totalSize > 1099511627776)
                UpdateStatus?.
                    Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {totalSize / 1099511627776d:F3} TiB)");
            else if(totalSize > 1073741824)
                UpdateStatus?.
                    Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {totalSize / 1073741824d:F3} GiB)");
            else if(totalSize > 1048576)
                UpdateStatus?.
                    Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {totalSize / 1048576d:F3} MiB)");
            else if(totalSize > 1024)
                UpdateStatus?.
                    Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {totalSize / 1024d:F3} KiB)");
            else
                UpdateStatus?.
                    Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {totalSize} bytes)");
        }

        // Check how many blocks to read, if error show and return
        // 64 works, gets maximum speed (150KiB/s), slow I know...
        if(scsiReader.GetBlocksToRead())
        {
            _dumpLog.WriteLine("ERROR: Cannot get blocks to read: {0}.", scsiReader.ErrorMessage);
            StoppingErrorMessage?.Invoke(scsiReader.ErrorMessage);

            return;
        }

        uint blocksToRead      = scsiReader.BlocksToRead;
        uint logicalBlockSize  = blockSize;
        uint physicalBlockSize = scsiReader.PhysicalBlockSize;

        if(blocks == 0)
        {
            _dumpLog.WriteLine("ERROR: Unable to read medium or empty medium present...");
            StoppingErrorMessage?.Invoke("Unable to read medium or empty medium present...");

            return;
        }

        UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");
        UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time.");
        UpdateStatus?.Invoke($"Device reports {blockSize} bytes per logical block.");
        UpdateStatus?.Invoke($"Device reports {scsiReader.LongBlockSize} bytes per physical block.");
        UpdateStatus?.Invoke($"SCSI device type: {_dev.ScsiType}.");
        UpdateStatus?.Invoke($"SCSI medium type: {scsiMediumType}.");
        UpdateStatus?.Invoke($"Media identified as {dskType}");

        _dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).", blocks, blocks * blockSize);
        _dumpLog.WriteLine("Device can read {0} blocks at a time.", blocksToRead);
        _dumpLog.WriteLine("Device reports {0} bytes per logical block.", blockSize);
        _dumpLog.WriteLine("Device reports {0} bytes per physical block.", scsiReader.LongBlockSize);
        _dumpLog.WriteLine("SCSI device type: {0}.", _dev.ScsiType);
        _dumpLog.WriteLine("SCSI medium type: {0}.", scsiMediumType);
        _dumpLog.WriteLine("Media identified as {0}.", dskType);

        sense = _dev.MiniDiscGetType(out cmdBuf, out _, _dev.Timeout, out _);

        if(!sense &&
           !_dev.Error)
            mediaTags.Add(MediaTagType.MiniDiscType, cmdBuf);

        sense = _dev.MiniDiscD5(out cmdBuf, out _, _dev.Timeout, out _);

        if(!sense &&
           !_dev.Error)
            mediaTags.Add(MediaTagType.MiniDiscD5, cmdBuf);

        sense = _dev.MiniDiscReadDataTOC(out cmdBuf, out _, _dev.Timeout, out _);

        if(!sense &&
           !_dev.Error)
            mediaTags.Add(MediaTagType.MiniDiscDTOC, cmdBuf);

        var utocMs = new MemoryStream();

        for(uint i = 0; i < 3; i++)
        {
            sense = _dev.MiniDiscReadUserTOC(out cmdBuf, out _, i, _dev.Timeout, out _);

            if(sense || _dev.Error)
                break;

            utocMs.Write(cmdBuf, 0, 2336);
        }

        if(utocMs.Length > 0)
            mediaTags.Add(MediaTagType.MiniDiscUTOC, utocMs.ToArray());

        ret = true;

        foreach(MediaTagType tag in mediaTags.Keys.Where(tag => !outputFormat.SupportedMediaTags.Contains(tag)))
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

        UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time.");
        _dumpLog.WriteLine("Reading {0} sectors at a time.", blocksToRead);

        var mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead, _private);
        var ibgLog  = new IbgLog(_outputPrefix  + ".ibg", sbcProfile);
        ret = outputFormat.Create(_outputPath, dskType, _formatOptions, blocks, blockSize);

        // Cannot create image
        if(!ret)
        {
            _dumpLog.WriteLine("Error creating output image, not continuing.");
            _dumpLog.WriteLine(outputFormat.ErrorMessage);

            StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                         outputFormat.ErrorMessage);

            return;
        }

        start = DateTime.UtcNow;
        double imageWriteDuration = 0;

        if(decMode?.Pages != null)
        {
            var setGeometry = false;

            foreach(Modes.ModePage page in decMode.Value.Pages)
                if(page.Page    == 0x04 &&
                   page.Subpage == 0x00)
                {
                    Modes.ModePage_04? rigidPage = Modes.DecodeModePage_04(page.PageResponse);

                    if(!rigidPage.HasValue || setGeometry)
                        continue;

                    _dumpLog.WriteLine("Setting geometry to {0} cylinders, {1} heads, {2} sectors per track",
                                       rigidPage.Value.Cylinders, rigidPage.Value.Heads,
                                       (uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads)));

                    UpdateStatus?.
                        Invoke($"Setting geometry to {rigidPage.Value.Cylinders} cylinders, {rigidPage.Value.Heads} heads, {(uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads))} sectors per track");

                    outputFormat.SetGeometry(rigidPage.Value.Cylinders, rigidPage.Value.Heads,
                                             (uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads)));

                    setGeometry = true;
                }
                else if(page.Page    == 0x05 &&
                        page.Subpage == 0x00)
                {
                    Modes.ModePage_05? flexiblePage = Modes.DecodeModePage_05(page.PageResponse);

                    if(!flexiblePage.HasValue)
                        continue;

                    _dumpLog.WriteLine("Setting geometry to {0} cylinders, {1} heads, {2} sectors per track",
                                       flexiblePage.Value.Cylinders, flexiblePage.Value.Heads,
                                       flexiblePage.Value.SectorsPerTrack);

                    UpdateStatus?.
                        Invoke($"Setting geometry to {flexiblePage.Value.Cylinders} cylinders, {flexiblePage.Value.Heads} heads, {flexiblePage.Value.SectorsPerTrack} sectors per track");

                    outputFormat.SetGeometry(flexiblePage.Value.Cylinders, flexiblePage.Value.Heads,
                                             flexiblePage.Value.SectorsPerTrack);

                    setGeometry = true;
                }
        }

        DumpHardwareType currentTry = null;
        ExtentsULong     extents    = null;

        ResumeSupport.Process(true, _dev.IsRemovable, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial,
                              _dev.PlatformId, ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision,
                              _private, _force);

        if(currentTry == null ||
           extents    == null)
        {
            StoppingErrorMessage?.Invoke("Could not process resume file, not continuing...");

            return;
        }

        if(_resume.NextBlock > 0)
        {
            UpdateStatus?.Invoke($"Resuming from block {_resume.NextBlock}.");
            _dumpLog.WriteLine("Resuming from block {0}.", _resume.NextBlock);
        }

        var      newTrim          = false;
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
                blocksToRead = (uint)(blocks - i);

            if(currentSpeed > maxSpeed &&
               currentSpeed > 0)
                maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed &&
               currentSpeed > 0)
                minSpeed = currentSpeed;

            UpdateProgress?.Invoke($"Reading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)", (long)i,
                                   (long)blocks);

            sense = _dev.Read6(out readBuffer, out _, (uint)i, blockSize, (byte)blocksToRead, _dev.Timeout,
                               out double cmdDuration);

            totalDuration += cmdDuration;

            if(!sense &&
               !_dev.Error)
            {
                mhddLog.Write(i, cmdDuration);
                ibgLog.Write(i, currentSpeed * 1024);
                DateTime writeStart = DateTime.Now;
                outputFormat.WriteSectors(readBuffer, i, blocksToRead);
                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                extents.Add(i, blocksToRead, true);
            }
            else
            {
                // TODO: Reset device after X errors
                if(_stopOnError)
                    return; // TODO: Return more cleanly

                if(i + _skip > blocks)
                    _skip = (uint)(blocks - i);

                // Write empty data
                DateTime writeStart = DateTime.Now;
                outputFormat.WriteSectors(new byte[blockSize * _skip], i, _skip);
                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                for(ulong b = i; b < i + _skip; b++)
                    _resume.BadBlocks.Add(b);

                mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                ibgLog.Write(i, 0);
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

        end = DateTime.UtcNow;
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

                sense = _dev.Read6(out readBuffer, out _, (uint)badSector, blockSize, 1, _dev.Timeout, out double _);

                if(sense || _dev.Error)
                    continue;

                _resume.BadBlocks.Remove(badSector);
                extents.Add(badSector);
                outputFormat.WriteSector(readBuffer, badSector);
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
            var pass              = 1;
            var forward           = true;
            var runningPersistent = false;

            Modes.ModePage? currentModePage = null;
            byte[]          md6;

            if(_persistent)
            {
                Modes.ModePage_01 pg;

                sense = _dev.ModeSense6(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                        _dev.Timeout, out _);

                if(!sense)
                {
                    Modes.DecodedMode? dcMode6 = Modes.DecodeMode6(readBuffer, _dev.ScsiType);

                    if(dcMode6?.Pages != null)
                        foreach(Modes.ModePage modePage in dcMode6.Value.Pages.Where(modePage =>
                                    modePage.Page == 0x01 && modePage.Subpage == 0x00))
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
                        Page         = 0x01,
                        Subpage      = 0x00,
                        PageResponse = Modes.EncodeModePage_01(pg)
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

                var md = new Modes.DecodedMode
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

                md6 = Modes.EncodeMode6(md, _dev.ScsiType);

                UpdateStatus?.Invoke("Sending MODE SELECT to drive (return damaged blocks).");
                _dumpLog.WriteLine("Sending MODE SELECT to drive (return damaged blocks).");
                sense = _dev.ModeSelect(md6, out byte[] senseBuf, true, false, _dev.Timeout, out _);

                if(sense)
                {
                    UpdateStatus?.
                        Invoke("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");

                    AaruConsole.DebugWriteLine("Error: {0}", Sense.PrettifySense(senseBuf));

                    _dumpLog.WriteLine("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");
                }
                else
                    runningPersistent = true;
            }

            InitProgress?.Invoke();
        repeatRetry:
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

                sense = _dev.Read6(out readBuffer, out _, (uint)badSector, blockSize, 1, _dev.Timeout,
                                   out double cmdDuration);

                totalDuration += cmdDuration;

                if(!sense &&
                   !_dev.Error)
                {
                    _resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                    outputFormat.WriteSector(readBuffer, badSector);
                    UpdateStatus?.Invoke($"Correctly retried block {badSector} in pass {pass}.");
                    _dumpLog.WriteLine("Correctly retried block {0} in pass {1}.", badSector, pass);
                }
                else if(runningPersistent)
                    outputFormat.WriteSector(readBuffer, badSector);
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

                goto repeatRetry;
            }

            if(runningPersistent && currentModePage.HasValue)
            {
                var md = new Modes.DecodedMode
                {
                    Header = new Modes.ModeHeader(),
                    Pages = new[]
                    {
                        currentModePage.Value
                    }
                };

                md6 = Modes.EncodeMode6(md, _dev.ScsiType);

                UpdateStatus?.Invoke("Sending MODE SELECT to drive (return device to previous status).");
                _dumpLog.WriteLine("Sending MODE SELECT to drive (return device to previous status).");
                _dev.ModeSelect(md6, out _, true, false, _dev.Timeout, out _);
            }

            EndProgress?.Invoke();
        }
        #endregion Error handling

        _resume.BadBlocks.Sort();

        foreach(ulong bad in _resume.BadBlocks)
            _dumpLog.WriteLine("Sector {0} could not be read.", bad);

        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        outputFormat.SetDumpHardware(_resume.Tries);

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
            var         inputPlugin = ImageFormat.Detect(filter) as IMediaImage;
            ErrorNumber opened      = inputPlugin.Open(filter);

            if(opened != ErrorNumber.NoError)
            {
                StoppingErrorMessage?.Invoke($"Error {opened} opening created image.");

                return;
            }

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
            end = DateTime.UtcNow;

            if(!_aborted)
            {
                totalChkDuration = (end - chkStart).TotalMilliseconds;
                UpdateStatus?.Invoke($"Sidecar created in {(end - chkStart).TotalSeconds} seconds.");

                UpdateStatus?.
                    Invoke($"Average checksum speed {blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000):F3} KiB/sec.");

                _dumpLog.WriteLine("Sidecar created in {0} seconds.", (end - chkStart).TotalSeconds);

                _dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                                   blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                if(_preSidecar != null)
                {
                    _preSidecar.BlockMedia = sidecar.BlockMedia;
                    sidecar                = _preSidecar;
                }

                List<(ulong start, string type)> filesystems = new();

                if(sidecar.BlockMedia[0].FileSystemInformation != null)
                    filesystems.AddRange(from partition in sidecar.BlockMedia[0].FileSystemInformation
                                         where partition.FileSystems != null from fileSystem in partition.FileSystems
                                         select (partition.StartSector, fileSystem.Type));

                if(filesystems.Count > 0)
                    foreach(var filesystem in filesystems.Select(o => new
                            {
                                o.start,
                                o.type
                            }).Distinct())
                    {
                        UpdateStatus?.Invoke($"Found filesystem {filesystem.type} at sector {filesystem.start}");
                        _dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type, filesystem.start);
                    }

                sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);
                (string type, string subType) xmlType = CommonTypes.Metadata.MediaType.MediaTypeToString(dskType);
                sidecar.BlockMedia[0].DiskType    = xmlType.type;
                sidecar.BlockMedia[0].DiskSubType = xmlType.subType;

                if(!_dev.IsRemovable ||
                   _dev.IsUsb)
                    if(_dev.Type == DeviceType.ATAPI)
                        sidecar.BlockMedia[0].Interface = "ATAPI";
                    else if(_dev.IsUsb)
                        sidecar.BlockMedia[0].Interface = "USB";
                    else if(_dev.IsFireWire)
                        sidecar.BlockMedia[0].Interface = "FireWire";
                    else
                        sidecar.BlockMedia[0].Interface = "SCSI";

                sidecar.BlockMedia[0].LogicalBlocks     = blocks;
                sidecar.BlockMedia[0].PhysicalBlockSize = physicalBlockSize;
                sidecar.BlockMedia[0].LogicalBlockSize  = logicalBlockSize;
                sidecar.BlockMedia[0].Manufacturer      = _dev.Manufacturer;
                sidecar.BlockMedia[0].Model             = _dev.Model;

                if(!_private)
                    sidecar.BlockMedia[0].Serial = _dev.Serial;

                sidecar.BlockMedia[0].Size = blocks * blockSize;

                if(_dev.IsRemovable)
                    sidecar.BlockMedia[0].DumpHardwareArray = _resume.Tries.ToArray();

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

        Statistics.AddMedia(dskType, true);
    }
}