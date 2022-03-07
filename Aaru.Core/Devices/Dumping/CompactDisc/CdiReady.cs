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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/



// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping;

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Logging;
using Aaru.Decoders.CD;
using Aaru.Devices;
using Schemas;

partial class Dump
{
    /// <summary>Detects if a sector contains data</summary>
    /// <param name="sector">Sector contents</param>
    /// <returns><c>true</c> if it contains Yellow Book data, <c>false</c> otherwise</returns>
    static bool IsData(byte[] sector)
    {
        if(sector?.Length != 2352)
            return false;

        byte[] syncMark =
        {
            0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00
        };

        var testMark = new byte[12];
        Array.Copy(sector, 0, testMark, 0, 12);

        return syncMark.SequenceEqual(testMark) && (sector[0xF] == 0 || sector[0xF] == 1 || sector[0xF] == 2);
    }

    /// <summary>Detects if a sector contains scrambled data</summary>
    /// <param name="sector">Sector contents</param>
    /// <param name="wantedLba">What LBA we intended to read</param>
    /// <param name="offset">Offset in bytes, if found</param>
    /// <returns><c>true</c> if it contains Yellow Book data, <c>false</c> otherwise</returns>
    static bool IsScrambledData(byte[] sector, int wantedLba, out int? offset)
    {
        offset = 0;

        if(sector?.Length != 2352)
            return false;

        byte[] syncMark =
        {
            0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00
        };

        var testMark = new byte[12];

        for(var i = 0; i <= 2336; i++)
        {
            Array.Copy(sector, i, testMark, 0, 12);

            if(!syncMark.SequenceEqual(testMark) ||
               sector[i + 0xF] != 0x60 && sector[i + 0xF] != 0x61 && sector[i + 0xF] != 0x62)
                continue;

            // De-scramble M and S
            int minute = sector[i + 12] ^ 0x01;
            int second = sector[i + 13] ^ 0x80;
            int frame  = sector[i + 14];

            // Convert to binary
            minute = minute / 16 * 10 + (minute & 0x0F);
            second = second / 16 * 10 + (second & 0x0F);
            frame  = frame  / 16 * 10 + (frame  & 0x0F);

            // Calculate the first found LBA
            int lba = minute * 60 * 75 + second * 75 + frame - 150;

            // Calculate the difference between the found LBA and the requested one
            int diff = wantedLba - lba;

            offset = i + 2352 * diff;

            return true;
        }

        return false;
    }

    // TODO: Set pregap for Track 1
    // TODO: Detect errors in sectors
    /// <summary>Reads all the hidden track in CD-i Ready discs</summary>
    /// <param name="blocks">Total number of positive sectors</param>
    /// <param name="blockSize">Size of the read sector in bytes</param>
    /// <param name="currentSpeed">Current read speed</param>
    /// <param name="currentTry">Current dump hardware try</param>
    /// <param name="extents">Extents</param>
    /// <param name="ibgLog">IMGBurn log</param>
    /// <param name="imageWriteDuration">Duration of image write</param>
    /// <param name="leadOutExtents">Lead-out extents</param>
    /// <param name="maxSpeed">Maximum speed</param>
    /// <param name="mhddLog">MHDD log</param>
    /// <param name="minSpeed">Minimum speed</param>
    /// <param name="offsetBytes">Read offset</param>
    /// <param name="sectorsForOffset">Sectors needed to fix offset</param>
    /// <param name="subSize">Subchannel size in bytes</param>
    /// <param name="supportedSubchannel">Drive's maximum supported subchannel</param>
    /// <param name="totalDuration">Total commands duration</param>
    /// <param name="cdiReadyReadAsAudio">Is the drive returning CD-i Ready hidden track as audio?</param>
    /// <param name="tracks">Disc tracks</param>
    /// <param name="subLog">Subchannel log</param>
    /// <param name="desiredSubchannel">Subchannel desired to save</param>
    /// <param name="isrcs">List of disc ISRCs</param>
    /// <param name="mcn">Disc media catalogue number</param>
    /// <param name="subchannelExtents">List of subchannels not yet dumped correctly</param>
    /// <param name="smallestPregapLbaPerTrack">List of smallest pregap relative address per track</param>
    void ReadCdiReady(uint blockSize, ref double currentSpeed, DumpHardwareType currentTry, ExtentsULong extents,
                      IbgLog ibgLog, ref double imageWriteDuration, ExtentsULong leadOutExtents, ref double maxSpeed,
                      MhddLog mhddLog, ref double minSpeed, uint subSize, MmcSubchannel supportedSubchannel,
                      ref double totalDuration, Track[] tracks, SubchannelLog subLog, MmcSubchannel desiredSubchannel,
                      Dictionary<byte, string> isrcs, ref string mcn, HashSet<int> subchannelExtents, ulong blocks,
                      bool cdiReadyReadAsAudio, int offsetBytes, int sectorsForOffset,
                      Dictionary<byte, int> smallestPregapLbaPerTrack)
    {
        ulong      sectorSpeedStart = 0;               // Used to calculate correct speed
        DateTime   timeSpeedStart   = DateTime.UtcNow; // Time of start for speed calculation
        bool       sense;                              // Sense indicator
        byte[]     cmdBuf;                             // Data buffer
        byte[]     senseBuf;                           // Sense buffer
        double     cmdDuration;                        // Command execution time
        const uint sectorSize = 2352;                  // Full sector size
        Track      firstTrack = tracks.FirstOrDefault(t => t.Sequence == 1);
        uint       blocksToRead; // How many sectors to read at once
        var        outputOptical = _outputPlugin as IWritableOpticalImage;

        if(firstTrack is null)
            return;

        if(cdiReadyReadAsAudio)
        {
            _dumpLog.WriteLine("Setting speed to 8x for CD-i Ready reading as audio.");
            UpdateStatus?.Invoke("Setting speed to 8x for CD-i Ready reading as audio.");

            _dev.SetCdSpeed(out _, RotationalControl.ClvAndImpureCav, 1416, 0, _dev.Timeout, out _);
        }

        InitProgress?.Invoke();

        for(ulong i = _resume.NextBlock; i < firstTrack.StartSector; i += blocksToRead)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke("Aborted!");
                _dumpLog.WriteLine("Aborted!");

                break;
            }

            var firstSectorToRead = (uint)i;

            blocksToRead = _maximumReadable;

            if(blocksToRead == 1 && cdiReadyReadAsAudio)
                blocksToRead += (uint)sectorsForOffset;

            if(cdiReadyReadAsAudio)
                if(offsetBytes < 0)
                {
                    if(i == 0)
                        firstSectorToRead = uint.MaxValue - (uint)(sectorsForOffset - 1); // -1
                    else
                        firstSectorToRead -= (uint)sectorsForOffset;
                }

            if(currentSpeed > maxSpeed &&
               currentSpeed > 0)
                maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed &&
               currentSpeed > 0)
                minSpeed = currentSpeed;

            UpdateProgress?.Invoke($"Reading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)", (long)i,
                                   (long)blocks);

            sense = _dev.ReadCd(out cmdBuf, out senseBuf, firstSectorToRead, blockSize, blocksToRead,
                                MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);

            totalDuration += cmdDuration;

            double elapsed;

            // Overcome the track mode change drive error
            if(sense)
                for(uint r = 0; r < _maximumReadable; r++)
                {
                    UpdateProgress?.Invoke($"Reading sector {i + r} of {blocks} ({currentSpeed:F3} MiB/sec.)",
                                           (long)i + r, (long)blocks);

                    sense = _dev.ReadCd(out cmdBuf, out senseBuf, (uint)(i + r), blockSize, (uint)sectorsForOffset + 1,
                                        MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                        true, MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);

                    totalDuration += cmdDuration;

                    if(!sense &&
                       !_dev.Error)
                    {
                        mhddLog.Write(i + r, cmdDuration);
                        ibgLog.Write(i  + r, currentSpeed * 1024);
                        extents.Add(i   + r, 1, true);
                        DateTime writeStart = DateTime.Now;

                        if(cdiReadyReadAsAudio)
                            FixOffsetData(offsetBytes, sectorSize, sectorsForOffset, supportedSubchannel,
                                          ref blocksToRead, subSize, ref cmdBuf, blockSize, false);

                        if(supportedSubchannel != MmcSubchannel.None)
                        {
                            var data = new byte[sectorSize];
                            var sub  = new byte[subSize];

                            Array.Copy(cmdBuf, 0, data, 0, sectorSize);

                            Array.Copy(cmdBuf, sectorSize, sub, 0, subSize);

                            if(cdiReadyReadAsAudio)
                                data = Sector.Scramble(data);

                            outputOptical.WriteSectorsLong(data, i + r, 1);

                            bool indexesChanged = Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel,
                                desiredSubchannel, sub, i + r, 1, subLog, isrcs, 1, ref mcn, tracks,
                                subchannelExtents, _fixSubchannelPosition, outputOptical, _fixSubchannel,
                                _fixSubchannelCrc, _dumpLog, UpdateStatus, smallestPregapLbaPerTrack, true);

                            // Set tracks and go back
                            if(indexesChanged)
                            {
                                outputOptical.SetTracks(tracks.ToList());
                                i -= _maximumReadable;

                                continue;
                            }
                        }
                        else
                            outputOptical.WriteSectorsLong(cmdBuf, i + r, 1);

                        imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                    }
                    else
                    {
                        _errorLog?.WriteLine(i + r, _dev.Error, _dev.LastError, senseBuf);

                        leadOutExtents.Add(i + r, firstTrack.StartSector - 1);

                        UpdateStatus?.
                            Invoke($"Adding CD-i Ready hole from LBA {i + r} to {firstTrack.StartSector - 1} inclusive.");

                        _dumpLog.WriteLine("Adding CD-i Ready hole from LBA {0} to {1} inclusive.", i + r,
                                           firstTrack.StartSector                                     - 1);

                        break;
                    }

                    sectorSpeedStart += r;

                    _resume.NextBlock = i + r;

                    elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                    if(elapsed <= 0)
                        continue;

                    currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                    sectorSpeedStart = 0;
                    timeSpeedStart   = DateTime.UtcNow;
                }

            if(!sense &&
               !_dev.Error)
            {
                if(cdiReadyReadAsAudio)
                    FixOffsetData(offsetBytes, sectorSize, sectorsForOffset, supportedSubchannel, ref blocksToRead,
                                  subSize, ref cmdBuf, blockSize, false);

                mhddLog.Write(i, cmdDuration);
                ibgLog.Write(i, currentSpeed * 1024);
                extents.Add(i, blocksToRead, true);
                DateTime writeStart = DateTime.Now;

                if(supportedSubchannel != MmcSubchannel.None)
                {
                    var data    = new byte[sectorSize * blocksToRead];
                    var sub     = new byte[subSize    * blocksToRead];
                    var tmpData = new byte[sectorSize];

                    for(var b = 0; b < blocksToRead; b++)
                    {
                        if(cdiReadyReadAsAudio)
                        {
                            Array.Copy(cmdBuf, (int)(0 + b * blockSize), tmpData, 0, sectorSize);
                            tmpData = Sector.Scramble(tmpData);
                            Array.Copy(tmpData, 0, data, sectorSize * b, sectorSize);
                        }
                        else
                            Array.Copy(cmdBuf, (int)(0 + b * blockSize), data, sectorSize * b, sectorSize);

                        Array.Copy(cmdBuf, (int)(sectorSize + b * blockSize), sub, subSize * b, subSize);
                    }

                    outputOptical.WriteSectorsLong(data, i, blocksToRead);

                    bool indexesChanged = Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel,
                        desiredSubchannel, sub, i, blocksToRead, subLog, isrcs, 1, ref mcn, tracks,
                        subchannelExtents, _fixSubchannelPosition, outputOptical, _fixSubchannel,
                        _fixSubchannelCrc, _dumpLog, UpdateStatus, smallestPregapLbaPerTrack, true);

                    // Set tracks and go back
                    if(indexesChanged)
                    {
                        outputOptical.SetTracks(tracks.ToList());
                        i -= blocksToRead;

                        continue;
                    }
                }
                else
                {
                    if(cdiReadyReadAsAudio)
                    {
                        var tmpData = new byte[sectorSize];
                        var data    = new byte[sectorSize * blocksToRead];

                        for(var b = 0; b < blocksToRead; b++)
                        {
                            Array.Copy(cmdBuf, (int)(b * sectorSize), tmpData, 0, sectorSize);
                            tmpData = Sector.Scramble(tmpData);
                            Array.Copy(tmpData, 0, data, sectorSize * b, sectorSize);
                        }

                        outputOptical.WriteSectorsLong(data, i, blocksToRead);
                    }
                    else
                        outputOptical.WriteSectorsLong(cmdBuf, i, blocksToRead);
                }

                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
            }
            else
            {
                _errorLog?.WriteLine(i, _dev.Error, _dev.LastError, senseBuf);

                _resume.NextBlock = firstTrack.StartSector;

                break;
            }

            sectorSpeedStart += blocksToRead;

            _resume.NextBlock = i + blocksToRead;

            elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

            if(elapsed <= 0)
                continue;

            currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
            sectorSpeedStart = 0;
            timeSpeedStart   = DateTime.UtcNow;
        }

        EndProgress?.Invoke();
    }
}