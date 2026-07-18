// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : RawBd.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
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
// Copyright © 2011-2026 Natalia Portillo
// Copyright © 2020-2026 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Linq;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Logging;
using Aaru.Logging;
using Humanizer;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>
    ///     Dumps raw Blu-ray sectors (2052-byte DataFrames) when dumping from a SCSI Block Commands compliant device.
    ///     Supports OmniDrive raw reading.
    /// </summary>
    /// <param name="blocks">Media blocks</param>
    /// <param name="maxBlocksToRead">Maximum number of blocks to read in a single command</param>
    /// <param name="blockSize">Block size in bytes</param>
    /// <param name="currentTry">Resume information</param>
    /// <param name="extents">Correctly dump extents</param>
    /// <param name="currentSpeed">Current speed</param>
    /// <param name="minSpeed">Minimum speed</param>
    /// <param name="maxSpeed">Maximum speed</param>
    /// <param name="totalDuration">Total time spent in commands</param>
    /// <param name="scsiReader">SCSI reader</param>
    /// <param name="mhddLog">MHDD log</param>
    /// <param name="ibgLog">ImgBurn log</param>
    /// <param name="imageWriteDuration">Total time spent writing to image</param>
    /// <param name="newTrim">Set if we need to start a trim</param>
    /// <param name="nominalNegativeSectors">Lead-in sectors to read when drive and format supports negative sectors</param>
    /// <param name="overflowSectors">Leadout sectors to read when drive and format supports overflow sectors</param>
    void ReadRawBdData(in ulong     blocks, in uint maxBlocksToRead, in uint blockSize, DumpHardware currentTry,
                       ExtentsULong extents, ref double currentSpeed, ref double minSpeed, ref double maxSpeed,
                       ref double   totalDuration, Reader scsiReader, MhddLog mhddLog, IbgLog ibgLog,
                       ref double   imageWriteDuration, ref bool newTrim, uint nominalNegativeSectors,
                       uint overflowSectors)
    {
        ulong  sectorSpeedStart = 0;
        bool   sense;
        byte[] buffer;
        uint   blocksToRead = maxBlocksToRead;
        var    outputFormat = _outputPlugin as IWritableImage;

        if(outputFormat is null)
        {
            ErrorMessage?.Invoke(Localization.Core.Output_format_not_initialized);
            return;
        }

        InitProgress?.Invoke();
        _speedStopwatch.Reset();
        double elapsed = 0;

        // Phase 1: Lead-in (negative LBA) — OmniDrive only. Read from nominalNegativeSectors up to -1.
        if(nominalNegativeSectors > 0)
        {
            UpdateStatus?.Invoke(Localization.Core.Reading_lead_in_sectors);

            for(ulong sectorAddress = nominalNegativeSectors; sectorAddress >= 1;)
            {
                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke(Localization.Core.Aborted);

                    break;
                }

                uint toRead = (uint)Math.Min(sectorAddress, blocksToRead);

                if(currentSpeed > maxSpeed && currentSpeed > 0) maxSpeed = currentSpeed;
                if(currentSpeed < minSpeed && currentSpeed > 0) minSpeed = currentSpeed;

                UpdateProgress?.Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2,
                                                     sectorAddress,
                                                     nominalNegativeSectors,
                                                     ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                                       (long)(nominalNegativeSectors - sectorAddress + toRead),
                                       (long)nominalNegativeSectors);

                _speedStopwatch.Restart();
                sense         = scsiReader.ReadBlocks(out buffer, sectorAddress, toRead, out double cmdDuration, out _,
                                                      out _, true);
                elapsed       += _speedStopwatch.Elapsed.TotalMilliseconds;
                totalDuration += cmdDuration;
                _speedStopwatch.Stop();

                if(!sense && !_dev.Error)
                {
                    mhddLog.Write((ulong)-(long)sectorAddress, cmdDuration, toRead);
                    ibgLog.Write((ulong)-(long)sectorAddress, currentSpeed * 1024);

                    // ReadBlocks returns sectors in logical order (-N..-1); WriteSectorsLong expects ascending order.
                    byte[] writeBuffer = new byte[buffer.Length];
                    for(uint i = 0; i < toRead; i++)
                        Array.Copy(buffer, (int)(i * blockSize), writeBuffer, (int)((toRead - 1 - i) * blockSize),
                                   (int)blockSize);

                    _writeStopwatch.Restart();
                    outputFormat.WriteSectorsLong(writeBuffer,
                                                  sectorAddress - toRead + 1,
                                                  true,
                                                  toRead,
                                                  Enumerable.Repeat(SectorStatus.Dumped, (int)toRead).ToArray());
                    imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                    _writeStopwatch.Stop();
                }
                else
                {
                    if(_stopOnError) return;

                    _writeStopwatch.Restart();
                    outputFormat.WriteSectorsLong(new byte[blockSize * toRead],
                                                  sectorAddress,
                                                  true,
                                                  toRead,
                                                  Enumerable.Repeat(SectorStatus.NotDumped, (int)toRead).ToArray());
                    imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                    _writeStopwatch.Stop();
                }

                if(sectorAddress <= 1) break;
                sectorAddress -= toRead;
                sectorSpeedStart += toRead;

                if(elapsed < 100) continue;

                currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed / 1000);
                ibgLog.Write((ulong)-(long)(sectorAddress + toRead), currentSpeed * 1024);
                sectorSpeedStart = 0;
                elapsed          = 0;
                _speedStopwatch.Reset();
            }

            UpdateStatus?.Invoke(Localization.Core.Reading_data_sectors);
        }

        // Phase 2: Data zone
        for(ulong i = _resume.NextBlock; i < blocks; i += blocksToRead)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);

                break;
            }

            if(blocks - i < blocksToRead) blocksToRead = (uint)(blocks - i);

            if(currentSpeed > maxSpeed && currentSpeed > 0) maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed && currentSpeed > 0) minSpeed = currentSpeed;

            UpdateProgress?.Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2,
                                                 i,
                                                 blocks,
                                                 ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                                   (long)i,
                                   (long)blocks);

            _speedStopwatch.Restart();
            sense         = scsiReader.ReadBlocks(out buffer, i, blocksToRead, out double cmdDuration, out _, out _);
            elapsed       += _speedStopwatch.Elapsed.TotalMilliseconds;
            totalDuration += cmdDuration;
            _speedStopwatch.Stop();

            if(!sense && !_dev.Error)
            {
                mhddLog.Write(i, cmdDuration, blocksToRead);
                ibgLog.Write(i, currentSpeed * 1024);

                _writeStopwatch.Restart();
                outputFormat.WriteSectorsLong(buffer,
                                              i,
                                              false,
                                              blocksToRead,
                                              Enumerable.Repeat(SectorStatus.Dumped, (int)blocksToRead).ToArray());

                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                extents.Add(i, blocksToRead, true);
                _mediaGraph?.PaintSectorsGood(i, blocksToRead);
            }
            else
            {
                if(_stopOnError) return;

                if(i + _skip > blocks) _skip = (uint)(blocks - i);

                _writeStopwatch.Restart();
                outputFormat.WriteSectorsLong(new byte[blockSize * _skip],
                                              i,
                                              false,
                                              _skip,
                                              Enumerable.Repeat(SectorStatus.NotDumped, (int)_skip).ToArray());

                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                for(ulong b = i; b < i + _skip; b++) _resume.BadBlocks.Add(b);

                mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration, _skip);
                ibgLog.Write(i, 0);
                AaruLogging.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, i);
                i       += _skip - blocksToRead;
                newTrim =  true;
            }

            _writeStopwatch.Stop();
            sectorSpeedStart  += blocksToRead;
            _resume.NextBlock =  i + blocksToRead;

            if(elapsed < 100) continue;

            currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed / 1000);
            ibgLog.Write(i, currentSpeed                                     * 1024);
            sectorSpeedStart = 0;
            elapsed          = 0;
            _speedStopwatch.Reset();
        }

        // Phase 3: Leadout (overflow sectors) — OmniDrive only
        if(overflowSectors > 0)
        {
            UpdateStatus?.Invoke(Localization.Core.Reading_lead_out_sectors);

            blocksToRead = maxBlocksToRead;
            for(ulong lba = blocks; lba < blocks + overflowSectors; lba += blocksToRead)
            {
                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke(Localization.Core.Aborted);

                    break;
                }

                uint toRead = (uint)(blocks + overflowSectors - lba);
                if(toRead > blocksToRead) toRead = blocksToRead;

                if(currentSpeed > maxSpeed && currentSpeed > 0) maxSpeed = currentSpeed;
                if(currentSpeed < minSpeed && currentSpeed > 0) minSpeed = currentSpeed;

                UpdateProgress?.Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2,
                                                     lba,
                                                     blocks + overflowSectors,
                                                     ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                                       (long)lba,
                                       (long)(blocks + overflowSectors));

                _speedStopwatch.Restart();
                sense         = scsiReader.ReadBlocks(out buffer, lba, toRead, out double cmdDuration, out _, out _);
                elapsed       += _speedStopwatch.Elapsed.TotalMilliseconds;
                totalDuration += cmdDuration;
                _speedStopwatch.Stop();

                if(!sense && !_dev.Error)
                {
                    mhddLog.Write(lba, cmdDuration, toRead);
                    ibgLog.Write(lba, currentSpeed * 1024);

                    _writeStopwatch.Restart();
                    outputFormat.WriteSectorsLong(buffer,
                                                  lba,
                                                  false,
                                                  toRead,
                                                  Enumerable.Repeat(SectorStatus.Dumped, (int)toRead).ToArray());
                    imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                }
                else
                {
                    if(_stopOnError) return;

                    _writeStopwatch.Restart();
                    outputFormat.WriteSectorsLong(new byte[blockSize * toRead],
                                                  lba,
                                                  false,
                                                  toRead,
                                                  Enumerable.Repeat(SectorStatus.NotDumped, (int)toRead).ToArray());
                    imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                }

                _writeStopwatch.Stop();
                sectorSpeedStart += toRead;

                if(elapsed < 100) continue;

                currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed / 1000);
                ibgLog.Write(lba, currentSpeed * 1024);
                sectorSpeedStart = 0;
                elapsed          = 0;
                _speedStopwatch.Reset();
            }
        }

        _speedStopwatch.Stop();
        _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

        EndProgress?.Invoke();
    }
}
