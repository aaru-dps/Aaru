// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LeadOuts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps CompactDisc Lead-Out.
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
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Logging;
using Aaru.Devices;
using Aaru.Logging;
using Humanizer;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Dumps inter-session lead-outs</summary>
    /// <param name="blockSize">Size of the read sector in bytes</param>
    /// <param name="currentTry">Current dump hardware try</param>
    /// <param name="extents">Extents</param>
    /// <param name="imageWriteDuration">Duration of image write</param>
    /// <param name="leadOutExtents">Lead-out extents</param>
    /// <param name="read6">Device supports READ(6)</param>
    /// <param name="read10">Device supports READ(10)</param>
    /// <param name="read12">Device supports READ(12)</param>
    /// <param name="read16">Device supports READ(16)</param>
    /// <param name="readcd">Device supports READ CD</param>
    /// <param name="supportedSubchannel">Drive's maximum supported subchannel</param>
    /// <param name="subSize">Subchannel size in bytes</param>
    /// <param name="totalDuration">Total commands duration</param>
    /// <param name="tracks">Disc tracks</param>
    /// <param name="subLog">Subchannel log</param>
    /// <param name="desiredSubchannel">Subchannel desired to save</param>
    /// <param name="isrcs">List of disc ISRCs</param>
    /// <param name="mcn">Disc media catalogue number</param>
    /// <param name="subchannelExtents">List of subchannels not yet dumped correctly</param>
    /// <param name="smallestPregapLbaPerTrack">List of smallest pregap relative address per track</param>
    void DumpCdLeadOuts(uint blockSize, DumpHardware currentTry, ExtentsULong extents, ref double imageWriteDuration,
                        ExtentsULong leadOutExtents, bool read6, bool read10, bool read12, bool read16,
                        bool readcd, MmcSubchannel supportedSubchannel, uint subSize, ref double totalDuration,
                        SubchannelLog subLog, MmcSubchannel desiredSubchannel, Dictionary<byte, string> isrcs,
                        ref string mcn, Track[] tracks, HashSet<int> subchannelExtents,
                        Dictionary<byte, int> smallestPregapLbaPerTrack, int offsetBytes, int sectorsForOffset)
    {
        byte[]             cmdBuf        = null; // Data buffer
        const uint         sectorSize    = 2352; // Full sector size
        var                sense         = true; // Sense indicator
        ReadOnlySpan<byte> senseBuf      = null;
        var                outputOptical = _outputPlugin as IWritableOpticalImage;
        SpeedMeter         speedMeter    = new(); // The lead-out is not part of the media speed, only for the progress
        double             leadOutSpeed  = 0;

        UpdateStatus?.Invoke(Localization.Core.Reading_lead_outs);

        InitProgress?.Invoke();

        foreach((ulong item1, ulong item2) in leadOutExtents.ToArray())
        {
            Track t = tracks.FirstOrDefault(t => item1 == t.EndSector + 1);

            bool isAudio               = t.Type == TrackType.Audio;
            bool needsOffsetCorrection = _fixOffset && isAudio && offsetBytes != 0;

            for(ulong i = item1; i <= item2; i++)
            {
                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke(Localization.Core.Aborted);

                    break;
                }

                double cmdDuration = 0;

                PulseProgress?.Invoke(string.Format(Localization.Core.Reading_sector_0_at_lead_out_1,
                                                    i,
                                                    ByteSize.FromMegabytes(leadOutSpeed).Per(_oneSecond).Humanize()));

                uint blocksToRead          = 1;
                var  firstSectorToRead     = (uint)i;
                var  failedCrossingLeadOut = false;

                if(needsOffsetCorrection)
                {
                    if(offsetBytes < 0)
                    {
                        firstSectorToRead =
                            i >= (ulong)sectorsForOffset ? (uint)(i - (ulong)sectorsForOffset) : (uint)i;

                        blocksToRead += (uint)sectorsForOffset;
                    }
                    else
                    {
                        blocksToRead += (uint)sectorsForOffset;

                        if(i + blocksToRead > item2 + 1) failedCrossingLeadOut = true;
                    }
                }

                if(_omnidrive)
                {
                    sense = _dev.OmniDriveReadCd(out cmdBuf,
                                                 out senseBuf,
                                                 firstSectorToRead,
                                                 blocksToRead,
                                                 _dev.Timeout,
                                                 out cmdDuration);
                }
                else if(readcd)
                {
                    sense = _dev.ReadCd(out cmdBuf,
                                        out senseBuf,
                                        firstSectorToRead,
                                        blockSize,
                                        blocksToRead,
                                        MmcSectorTypes.AllTypes,
                                        false,
                                        false,
                                        true,
                                        MmcHeaderCodes.AllHeaders,
                                        true,
                                        true,
                                        MmcErrorField.None,
                                        supportedSubchannel,
                                        _dev.Timeout,
                                        out cmdDuration);

                    totalDuration += cmdDuration;
                }
                else if(read16)
                {
                    sense = _dev.Read16(out cmdBuf,
                                        out senseBuf,
                                        0,
                                        false,
                                        true,
                                        false,
                                        firstSectorToRead,
                                        blockSize,
                                        0,
                                        blocksToRead,
                                        false,
                                        _dev.Timeout,
                                        out cmdDuration);
                }
                else if(read12)
                {
                    sense = _dev.Read12(out cmdBuf,
                                        out senseBuf,
                                        0,
                                        false,
                                        true,
                                        false,
                                        false,
                                        firstSectorToRead,
                                        blockSize,
                                        0,
                                        blocksToRead,
                                        false,
                                        _dev.Timeout,
                                        out cmdDuration);
                }
                else if(read10)
                {
                    sense = _dev.Read10(out cmdBuf,
                                        out senseBuf,
                                        0,
                                        false,
                                        true,
                                        false,
                                        false,
                                        firstSectorToRead,
                                        blockSize,
                                        0,
                                        (ushort)blocksToRead,
                                        _dev.Timeout,
                                        out cmdDuration);
                }
                else if(read6)
                {
                    sense = _dev.Read6(out cmdBuf,
                                       out senseBuf,
                                       firstSectorToRead,
                                       blockSize,
                                       (byte)blocksToRead,
                                       _dev.Timeout,
                                       out cmdDuration);
                }

                if(!sense && !_dev.Error)
                {
                    if(needsOffsetCorrection)
                    {
                        FixOffsetData(offsetBytes,
                                      sectorSize,
                                      sectorsForOffset,
                                      supportedSubchannel,
                                      ref blocksToRead,
                                      subSize,
                                      ref cmdBuf,
                                      blockSize,
                                      failedCrossingLeadOut);
                    }

                    extents.Add(i, blocksToRead, true);
                    leadOutExtents.Remove(i);
                    _writeStopwatch.Restart();

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        var data = new byte[sectorSize * blocksToRead];
                        var sub  = new byte[subSize    * blocksToRead];

                        for(var b = 0; b < blocksToRead; b++)
                        {
                            Array.Copy(cmdBuf, (int)(0 + b * blockSize), data, sectorSize * b, sectorSize);

                            Array.Copy(cmdBuf, (int)(sectorSize + b * blockSize), sub, subSize * b, subSize);
                        }

                        outputOptical.WriteSectorsLong(data,
                                                       i,
                                                       false,
                                                       blocksToRead,
                                                       [.. Enumerable.Repeat(SectorStatus.Dumped, (int)blocksToRead)]);

                        bool indexesChanged = Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel,
                            desiredSubchannel,
                            sub,
                            i,
                            blocksToRead,
                            subLog,
                            isrcs,
                            0xAA,
                            ref mcn,
                            tracks,
                            subchannelExtents,
                            _fixSubchannelPosition,
                            outputOptical,
                            _fixSubchannel,
                            _fixSubchannelCrc,
                            UpdateStatus,
                            smallestPregapLbaPerTrack,
                            true,
                            out _);

                        // Set tracks and go back
                        if(indexesChanged)
                        {
                            outputOptical.SetTracks([.. tracks]);
                            i--;

                            continue;
                        }
                    }
                    else
                    {
                        outputOptical.WriteSectors(cmdBuf,
                                                   i,
                                                   false,
                                                   blocksToRead,
                                                   [.. Enumerable.Repeat(SectorStatus.Dumped, (int)blocksToRead)]);
                    }

                    imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                }
                else
                {
                    // It's not going to get more lead-out sectors if it already failed.
                    break;
                }

                _writeStopwatch.Stop();
                speedMeter.Update(blockSize, ref leadOutSpeed);

                _resume.NextBlock = i + 1;
            }
        }

        EndProgress?.Invoke();
    }
}