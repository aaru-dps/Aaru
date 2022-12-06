// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MemoryStick.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Dumping with a jail-broken PlayStation Portable thru USB.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps a MemoryStick card using a jail-broken PlayStation Portable
//     thru USB.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Console;
using Aaru.Core.Logging;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Schemas;
using MediaType = Aaru.CommonTypes.MediaType;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Devices.Dumping
{
    public partial class Dump
    {
        [SuppressMessage("ReSharper", "JoinDeclarationAndInitializer")]
        void DumpMs()
        {
            const ushort sbcProfile    = 0x0001;
            const uint   blockSize     = 512;
            double       totalDuration = 0;
            double       currentSpeed  = 0;
            double       maxSpeed      = double.MinValue;
            double       minSpeed      = double.MaxValue;
            uint         blocksToRead  = 64;
            DateTime     start;
            DateTime     end;
            MediaType    dskType;
            bool         sense;
            byte[]       senseBuf;

            sense = _dev.ReadCapacity(out byte[] readBuffer, out _, _dev.Timeout, out _);

            if(sense)
            {
                _dumpLog.WriteLine("Could not detect capacity...");
                StoppingErrorMessage?.Invoke("Could not detect capacity...");

                return;
            }

            uint blocks = (uint)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3]);

            blocks++;

            ulong totalSize = blocks * (ulong)blockSize;

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

            if(blocks == 0)
            {
                _dumpLog.WriteLine("ERROR: Unable to read medium or empty medium present...");
                StoppingErrorMessage?.Invoke("Unable to read medium or empty medium present...");

                return;
            }

            UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");
            UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time.");
            UpdateStatus?.Invoke($"Device reports {blockSize} bytes per logical block.");
            UpdateStatus?.Invoke($"SCSI device type: {_dev.ScsiType}.");

            if(blocks > 262144)
            {
                dskType = MediaType.MemoryStickProDuo;
                _dumpLog.WriteLine("Media detected as MemoryStick Pro Duo...");
                UpdateStatus?.Invoke("Media detected as MemoryStick Pro Duo...");
            }
            else
            {
                dskType = MediaType.MemoryStickDuo;
                _dumpLog.WriteLine("Media detected as MemoryStick Duo...");
                UpdateStatus?.Invoke("Media detected as MemoryStick Duo...");
            }

            bool ret;

            var mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead, _private);
            var ibgLog  = new IbgLog(_outputPrefix  + ".ibg", sbcProfile);
            ret = _outputPlugin.Create(_outputPath, dskType, _formatOptions, blocks, blockSize);

            // Cannot create image
            if(!ret)
            {
                _dumpLog.WriteLine("Error creating output image, not continuing.");
                _dumpLog.WriteLine(_outputPlugin.ErrorMessage);

                StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                             _outputPlugin.ErrorMessage);

                return;
            }

            start = DateTime.UtcNow;
            double imageWriteDuration = 0;

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
                _dumpLog.WriteLine("Resuming from block {0}.", _resume.NextBlock);

            bool newTrim = false;

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

                UpdateProgress?.Invoke($"Reading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)", (long)i, blocks);

                sense = _dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)i, blockSize, 0,
                                    blocksToRead, false, _dev.Timeout, out double cmdDuration);

                totalDuration += cmdDuration;

                if(!sense &&
                   !_dev.Error)
                {
                    mhddLog.Write(i, cmdDuration);
                    ibgLog.Write(i, currentSpeed * 1024);
                    DateTime writeStart = DateTime.Now;
                    _outputPlugin.WriteSectors(readBuffer, i, blocksToRead);
                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                    extents.Add(i, blocksToRead, true);
                }
                else
                {
                    _errorLog?.WriteLine(i, _dev.Error, _dev.LastError, senseBuf);

                    // TODO: Reset device after X errors
                    if(_stopOnError)
                        return; // TODO: Return more cleanly

                    if(i + _skip > blocks)
                        _skip = (uint)(blocks - i);

                    // Write empty data
                    DateTime writeStart = DateTime.Now;
                    _outputPlugin.WriteSectors(new byte[blockSize * _skip], i, _skip);
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

                    sense = _dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)badSector,
                                        blockSize, 0, 1, false, _dev.Timeout, out double _);

                    if(sense || _dev.Error)
                    {
                        _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf);

                        continue;
                    }

                    _resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                    _outputPlugin.WriteSector(readBuffer, badSector);
                }

                EndProgress?.Invoke();
                end = DateTime.UtcNow;
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

                Modes.ModePage? currentModePage = null;
                byte[]          md6;

                if(_persistent)
                {
                    Modes.ModePage_01 pg;

                    sense = _dev.ModeSense6(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                            _dev.Timeout, out _);

                    if(sense)
                    {
                        sense = _dev.ModeSense10(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                                 _dev.Timeout, out _);

                        if(!sense)
                        {
                            Modes.DecodedMode? dcMode10 = Modes.DecodeMode10(readBuffer, _dev.ScsiType);

                            if(dcMode10.HasValue)
                                foreach(Modes.ModePage modePage in dcMode10.Value.Pages.Where(modePage =>
                                    modePage.Page == 0x01 && modePage.Subpage == 0x00))
                                    currentModePage = modePage;
                        }
                    }
                    else
                    {
                        Modes.DecodedMode? dcMode6 = Modes.DecodeMode6(readBuffer, _dev.ScsiType);

                        if(dcMode6.HasValue)
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
                    sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

                    if(sense)
                    {
                        UpdateStatus?.
                            Invoke("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");

                        AaruConsole.DebugWriteLine("Error: {0}", Sense.PrettifySense(senseBuf));

                        _dumpLog.
                            WriteLine("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");
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
                        _dumpLog.WriteLine("Aborted!");

                        break;
                    }

                    PulseProgress?.Invoke(string.Format("Retrying sector {0}, pass {1}, {3}{2}", badSector, pass,
                                                        forward ? "forward" : "reverse",
                                                        runningPersistent ? "recovering partial data, " : ""));

                    sense = _dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)badSector,
                                        blockSize, 0, 1, false, _dev.Timeout, out double cmdDuration);

                    totalDuration += cmdDuration;

                    if(sense || _dev.Error)
                        _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf);

                    if(!sense &&
                       !_dev.Error)
                    {
                        _resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        _outputPlugin.WriteSector(readBuffer, badSector);
                        UpdateStatus?.Invoke($"Correctly retried block {badSector} in pass {pass}.");
                        _dumpLog.WriteLine("Correctly retried block {0} in pass {1}.", badSector, pass);
                    }
                    else if(runningPersistent)
                        _outputPlugin.WriteSector(readBuffer, badSector);
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

            var metadata = new CommonTypes.Structs.ImageInfo
            {
                Application        = "Aaru",
                ApplicationVersion = Version.GetVersion()
            };

            if(!_outputPlugin.SetMetadata(metadata))
                ErrorMessage?.Invoke("Error {0} setting metadata, continuing..." + Environment.NewLine +
                                     _outputPlugin.ErrorMessage);

            _outputPlugin.SetDumpHardware(_resume.Tries);

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
                IMediaImage inputPlugin = ImageFormat.Detect(filter);

                if(!inputPlugin.Open(filter))
                {
                    StoppingErrorMessage?.Invoke("Could not open created image.");

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

                    List<(ulong start, string type)> filesystems = new List<(ulong start, string type)>();

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
                            UpdateStatus?.Invoke($"Found filesystem {filesystem.type} at sector {filesystem.start}");
                            _dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type, filesystem.start);
                        }

                    sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);
                    (string type, string subType) xmlType = CommonTypes.Metadata.MediaType.MediaTypeToString(dskType);
                    sidecar.BlockMedia[0].DiskType          = xmlType.type;
                    sidecar.BlockMedia[0].DiskSubType       = xmlType.subType;
                    sidecar.BlockMedia[0].Interface         = "USB";
                    sidecar.BlockMedia[0].LogicalBlocks     = blocks;
                    sidecar.BlockMedia[0].PhysicalBlockSize = (int)blockSize;
                    sidecar.BlockMedia[0].LogicalBlockSize  = (int)blockSize;
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
}