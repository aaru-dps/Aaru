// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Data.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps user data part.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Logging;
using Aaru.Devices;
using Schemas;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping
{
    partial class Dump
    {
        /// <summary>Reads all CD user data</summary>
        /// <param name="audioExtents">Extents with audio sectors</param>
        /// <param name="blocks">Total number of positive sectors</param>
        /// <param name="blockSize">Size of the read sector in bytes</param>
        /// <param name="currentSpeed">Current read speed</param>
        /// <param name="currentTry">Current dump hardware try</param>
        /// <param name="extents">Extents</param>
        /// <param name="ibgLog">IMGBurn log</param>
        /// <param name="imageWriteDuration">Duration of image write</param>
        /// <param name="lastSector">Last sector number</param>
        /// <param name="leadOutExtents">Lead-out extents</param>
        /// <param name="maxSpeed">Maximum speed</param>
        /// <param name="mhddLog">MHDD log</param>
        /// <param name="minSpeed">Minimum speed</param>
        /// <param name="newTrim">Is trim a new one?</param>
        /// <param name="nextData">Next cluster of sectors is all data</param>
        /// <param name="offsetBytes">Read offset</param>
        /// <param name="read6">Device supports READ(6)</param>
        /// <param name="read10">Device supports READ(10)</param>
        /// <param name="read12">Device supports READ(12)</param>
        /// <param name="read16">Device supports READ(16)</param>
        /// <param name="readcd">Device supports READ CD</param>
        /// <param name="sectorsForOffset">Sectors needed to fix offset</param>
        /// <param name="subSize">Subchannel size in bytes</param>
        /// <param name="supportedSubchannel">Drive's maximum supported subchannel</param>
        /// <param name="supportsLongSectors">Supports reading EDC and ECC</param>
        /// <param name="totalDuration">Total commands duration</param>
        void ReadCdiReady(uint blockSize, ref double currentSpeed, DumpHardwareType currentTry, ExtentsULong extents,
                          IbgLog ibgLog, ref double imageWriteDuration, ExtentsULong leadOutExtents,
                          ref double maxSpeed, MhddLog mhddLog, ref double minSpeed, bool read6, bool read10,
                          bool read12, bool read16, bool readcd, uint subSize, MmcSubchannel supportedSubchannel,
                          bool supportsLongSectors, ref double totalDuration, Track[] tracks, SubchannelLog subLog,
                          MmcSubchannel desiredSubchannel, Dictionary<byte, string> isrcs, ref string mcn,
                          HashSet<int> subchannelExtents, ulong blocks)
        {
            ulong      sectorSpeedStart = 0;               // Used to calculate correct speed
            DateTime   timeSpeedStart   = DateTime.UtcNow; // Time of start for speed calculation
            bool       sense            = true;            // Sense indicator
            byte[]     cmdBuf           = null;            // Data buffer
            byte[]     senseBuf         = null;            // Sense buffer
            double     cmdDuration      = 0;               // Command execution time
            const uint sectorSize       = 2352;            // Full sector size
            Track      firstTrack       = tracks.FirstOrDefault(t => t.TrackSequence == 1);

            if(firstTrack is null)
                return;

            InitProgress?.Invoke();

            for(ulong i = _resume.NextBlock; i < firstTrack.TrackStartSector; i += _maximumReadable)
            {
                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke("Aborted!");
                    _dumpLog.WriteLine("Aborted!");

                    break;
                }

                if(i >= firstTrack.TrackStartSector)
                    break;

                uint firstSectorToRead = (uint)i;

                Track track = tracks.OrderBy(t => t.TrackStartSector).LastOrDefault(t => i >= t.TrackStartSector);

                #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

                // ReSharper disable CompareOfFloatsByEqualityOperator
                if(currentSpeed > maxSpeed &&
                   currentSpeed != 0)
                    maxSpeed = currentSpeed;

                if(currentSpeed < minSpeed &&
                   currentSpeed != 0)
                    minSpeed = currentSpeed;

                // ReSharper restore CompareOfFloatsByEqualityOperator

                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                UpdateProgress?.Invoke($"Reading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)", (long)i,
                                       (long)blocks);

                if(readcd)
                {
                    sense = _dev.ReadCd(out cmdBuf, out senseBuf, firstSectorToRead, blockSize, _maximumReadable,
                                        MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                        true, MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);

                    totalDuration += cmdDuration;
                }
                else if(read16)
                {
                    sense = _dev.Read16(out cmdBuf, out senseBuf, 0, false, true, false, firstSectorToRead, blockSize,
                                        0, _maximumReadable, false, _dev.Timeout, out cmdDuration);
                }
                else if(read12)
                {
                    sense = _dev.Read12(out cmdBuf, out senseBuf, 0, false, true, false, false, firstSectorToRead,
                                        blockSize, 0, _maximumReadable, false, _dev.Timeout, out cmdDuration);
                }
                else if(read10)
                {
                    sense = _dev.Read10(out cmdBuf, out senseBuf, 0, false, true, false, false, firstSectorToRead,
                                        blockSize, 0, (ushort)_maximumReadable, _dev.Timeout, out cmdDuration);
                }
                else if(read6)
                {
                    sense = _dev.Read6(out cmdBuf, out senseBuf, firstSectorToRead, blockSize, (byte)_maximumReadable,
                                       _dev.Timeout, out cmdDuration);
                }

                double elapsed;

                // Overcome the track mode change drive error
                if(sense)
                {
                    for(uint r = 0; r < _maximumReadable; r++)
                    {
                        UpdateProgress?.Invoke($"Reading sector {i + r} of {blocks} ({currentSpeed:F3} MiB/sec.)",
                                               (long)i + r, (long)blocks);

                        if(readcd)
                        {
                            sense = _dev.ReadCd(out cmdBuf, out senseBuf, (uint)(i + r), blockSize, 1,
                                                MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                                true, true, MmcErrorField.None, supportedSubchannel, _dev.Timeout,
                                                out cmdDuration);

                            totalDuration += cmdDuration;
                        }
                        else if(read16)
                        {
                            sense = _dev.Read16(out cmdBuf, out senseBuf, 0, false, true, false, i + r, blockSize, 0, 1,
                                                false, _dev.Timeout, out cmdDuration);
                        }
                        else if(read12)
                        {
                            sense = _dev.Read12(out cmdBuf, out senseBuf, 0, false, true, false, false, (uint)(i + r),
                                                blockSize, 0, 1, false, _dev.Timeout, out cmdDuration);
                        }
                        else if(read10)
                        {
                            sense = _dev.Read10(out cmdBuf, out senseBuf, 0, false, true, false, false, (uint)(i + r),
                                                blockSize, 0, 1, _dev.Timeout, out cmdDuration);
                        }
                        else if(read6)
                        {
                            sense = _dev.Read6(out cmdBuf, out senseBuf, (uint)(i + r), blockSize, 1, _dev.Timeout,
                                               out cmdDuration);
                        }

                        if(!sense &&
                           !_dev.Error)
                        {
                            mhddLog.Write(i + r, cmdDuration);
                            ibgLog.Write(i  + r, currentSpeed * 1024);
                            extents.Add(i   + r, 1, true);
                            DateTime writeStart = DateTime.Now;

                            if(supportedSubchannel != MmcSubchannel.None)
                            {
                                byte[] data = new byte[sectorSize];
                                byte[] sub  = new byte[subSize];

                                Array.Copy(cmdBuf, 0, data, 0, sectorSize);

                                Array.Copy(cmdBuf, sectorSize, sub, 0, subSize);

                                _outputPlugin.WriteSectorsLong(data, i + r, 1);

                                bool indexesChanged =
                                    WriteSubchannelToImage(supportedSubchannel, desiredSubchannel, sub, i + r, 1,
                                                           subLog, isrcs, (byte)track.TrackSequence, ref mcn, tracks,
                                                           subchannelExtents);

                                // Set tracks and go back
                                if(indexesChanged)
                                {
                                    (_outputPlugin as IWritableOpticalImage).SetTracks(tracks.ToList());
                                    i -= _maximumReadable;

                                    continue;
                                }
                            }
                            else
                            {
                                if(supportsLongSectors)
                                {
                                    _outputPlugin.WriteSectorsLong(cmdBuf, i + r, 1);
                                }
                                else
                                {
                                    if(cmdBuf.Length % sectorSize == 0)
                                    {
                                        byte[] data = new byte[2048];

                                        Array.Copy(cmdBuf, 16, data, 2048, 2048);

                                        _outputPlugin.WriteSectors(data, i + r, 1);
                                    }
                                    else
                                    {
                                        _outputPlugin.WriteSectorsLong(cmdBuf, i + r, 1);
                                    }
                                }
                            }

                            imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                        }
                        else
                        {
                            leadOutExtents.Add(i + r, firstTrack.TrackStartSector - 1);

                            UpdateStatus?.
                                Invoke($"Adding CD-i Ready hole from LBA {i + r} to {firstTrack.TrackStartSector - 1} inclusive.");

                            _dumpLog.WriteLine("Adding CD-i Ready hole from LBA {0} to {1} inclusive.", i + r,
                                               firstTrack.TrackStartSector                                - 1);

                            break;
                        }

                        sectorSpeedStart += r;

                        _resume.NextBlock = i + r;

                        elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                        if(elapsed < 1)
                            continue;

                        currentSpeed     = (sectorSpeedStart * blockSize) / (1048576 * elapsed);
                        sectorSpeedStart = 0;
                        timeSpeedStart   = DateTime.UtcNow;
                    }
                }

                if(!sense &&
                   !_dev.Error)
                {
                    mhddLog.Write(i, cmdDuration);
                    ibgLog.Write(i, currentSpeed * 1024);
                    extents.Add(i, _maximumReadable, true);
                    DateTime writeStart = DateTime.Now;

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        byte[] data = new byte[sectorSize * _maximumReadable];
                        byte[] sub  = new byte[subSize    * _maximumReadable];

                        for(int b = 0; b < _maximumReadable; b++)
                        {
                            Array.Copy(cmdBuf, (int)(0 + (b * blockSize)), data, sectorSize * b, sectorSize);

                            Array.Copy(cmdBuf, (int)(sectorSize + (b * blockSize)), sub, subSize * b, subSize);
                        }

                        _outputPlugin.WriteSectorsLong(data, i, _maximumReadable);

                        bool indexesChanged = WriteSubchannelToImage(supportedSubchannel, desiredSubchannel, sub, i,
                                                                     _maximumReadable, subLog, isrcs,
                                                                     (byte)track.TrackSequence, ref mcn, tracks,
                                                                     subchannelExtents);

                        // Set tracks and go back
                        if(indexesChanged)
                        {
                            (_outputPlugin as IWritableOpticalImage).SetTracks(tracks.ToList());
                            i -= _maximumReadable;

                            continue;
                        }
                    }
                    else
                    {
                        if(supportsLongSectors)
                        {
                            _outputPlugin.WriteSectorsLong(cmdBuf, i, _maximumReadable);
                        }
                        else
                        {
                            if(cmdBuf.Length % sectorSize == 0)
                            {
                                byte[] data = new byte[2048 * _maximumReadable];

                                for(int b = 0; b < _maximumReadable; b++)
                                    Array.Copy(cmdBuf, (int)(16 + (b * blockSize)), data, 2048 * b, 2048);

                                _outputPlugin.WriteSectors(data, i, _maximumReadable);
                            }
                            else
                            {
                                _outputPlugin.WriteSectorsLong(cmdBuf, i, _maximumReadable);
                            }
                        }
                    }

                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                }
                else
                {
                    _resume.NextBlock = firstTrack.TrackStartSector;

                    break;
                }

                sectorSpeedStart += _maximumReadable;

                _resume.NextBlock = i + _maximumReadable;

                elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                if(elapsed < 1)
                    continue;

                currentSpeed     = (sectorSpeedStart * blockSize) / (1048576 * elapsed);
                sectorSpeedStart = 0;
                timeSpeedStart   = DateTime.UtcNow;
            }

            EndProgress?.Invoke();
        }
    }
}