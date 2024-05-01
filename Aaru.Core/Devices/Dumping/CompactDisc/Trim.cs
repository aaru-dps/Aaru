// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Trim.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Trims skipped sectors when dumping CompactDiscs.
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

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Logging;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Humanizer;
using Humanizer.Localisation;
using Track = Aaru.CommonTypes.Structs.Track;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Trims errored sectors in a CompactDisc</summary>
    /// <param name="audioExtents">Extents with audio sectors</param>
    /// <param name="blockSize">Size of the read sector in bytes</param>
    /// <param name="currentTry">Current dump hardware try</param>
    /// <param name="extents">Extents</param>
    /// <param name="newTrim">Is trim a new one?</param>
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
    /// <param name="tracks">Disc tracks</param>
    /// <param name="subLog">Subchannel log</param>
    /// <param name="desiredSubchannel">Subchannel desired to save</param>
    /// <param name="isrcs">List of disc ISRCs</param>
    /// <param name="mcn">Disc media catalogue number</param>
    /// <param name="subchannelExtents">List of subchannels not yet dumped correctly</param>
    /// <param name="smallestPregapLbaPerTrack">List of smallest pregap relative address per track</param>
    void TrimCdUserData(ExtentsULong audioExtents, uint blockSize, DumpHardware currentTry, ExtentsULong extents,
                        bool newTrim, int offsetBytes, bool read6, bool read10, bool read12, bool read16, bool readcd,
                        int sectorsForOffset, uint subSize, MmcSubchannel supportedSubchannel, bool supportsLongSectors,
                        ref double totalDuration, SubchannelLog subLog, MmcSubchannel desiredSubchannel, Track[] tracks,
                        Dictionary<byte, string> isrcs, ref string mcn, HashSet<int> subchannelExtents,
                        Dictionary<byte, int> smallestPregapLbaPerTrack)
    {
        var               sense       = true; // Sense indicator
        byte[]            cmdBuf      = null; // Data buffer
        double            cmdDuration = 0;    // Command execution time
        const uint        sectorSize  = 2352; // Full sector size
        PlextorSubchannel supportedPlextorSubchannel;
        byte[]            senseBuf      = null;
        var               outputOptical = _outputPlugin as IWritableOpticalImage;

        supportedPlextorSubchannel = supportedSubchannel switch
                                     {
                                         MmcSubchannel.None => PlextorSubchannel.None,
                                         MmcSubchannel.Raw  => PlextorSubchannel.Pack,
                                         MmcSubchannel.Q16  => PlextorSubchannel.Q16,
                                         _                  => PlextorSubchannel.None
                                     };

        if(_resume.BadBlocks.Count <= 0 || _aborted || !_trim || !newTrim) return;

        UpdateStatus?.Invoke(Localization.Core.Trimming_skipped_sectors);
        _dumpLog.WriteLine(Localization.Core.Trimming_skipped_sectors);
        InitProgress?.Invoke();
        _trimStopwatch.Restart();

    trimStart:
        ulong[] tmpArray = _resume.BadBlocks.ToArray();

        for(var b = 0; b < tmpArray.Length; b++)
        {
            ulong badSector = tmpArray[b];

            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);
                _dumpLog.WriteLine(Localization.Core.Aborted);

                break;
            }

            PulseProgress?.Invoke(string.Format(Localization.Core.Trimming_sector_0, badSector));

            Track track = tracks.OrderBy(t => t.StartSector).LastOrDefault(t => badSector >= t.StartSector);

            byte sectorsToTrim   = 1;
            var  badSectorToRead = (uint)badSector;

            if(_fixOffset && audioExtents.Contains(badSector) && offsetBytes != 0)
            {
                if(offsetBytes < 0)
                {
                    if(badSectorToRead == 0)
                        badSectorToRead = uint.MaxValue - (uint)(sectorsForOffset - 1); // -1
                    else
                        badSectorToRead -= (uint)sectorsForOffset;
                }

                sectorsToTrim += (byte)sectorsForOffset;
            }

            if(_supportsPlextorD8 && audioExtents.Contains(badSector))
            {
                sense = ReadPlextorWithSubchannel(out cmdBuf,
                                                  out senseBuf,
                                                  badSectorToRead,
                                                  blockSize,
                                                  sectorsToTrim,
                                                  supportedPlextorSubchannel,
                                                  out cmdDuration);
            }
            else if(readcd)
            {
                if(audioExtents.Contains(badSector))
                {
                    sense = _dev.ReadCd(out cmdBuf,
                                        out senseBuf,
                                        badSectorToRead,
                                        blockSize,
                                        sectorsToTrim,
                                        MmcSectorTypes.Cdda,
                                        false,
                                        false,
                                        false,
                                        MmcHeaderCodes.None,
                                        true,
                                        false,
                                        MmcErrorField.None,
                                        supportedSubchannel,
                                        _dev.Timeout,
                                        out cmdDuration);

                    if(sense)
                    {
                        DecodedSense? decSense = Sense.Decode(senseBuf);

                        // Try to workaround firmware
                        if(decSense is { ASC: 0x11, ASCQ: 0x05 } || decSense?.ASC == 0x64)
                        {
                            sense = _dev.ReadCd(out cmdBuf,
                                                out _,
                                                badSectorToRead,
                                                blockSize,
                                                sectorsToTrim,
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
                                                out double cmdDuration2);

                            cmdDuration += cmdDuration2;
                        }
                    }
                }
                else
                {
                    sense = _dev.ReadCd(out cmdBuf,
                                        out senseBuf,
                                        badSectorToRead,
                                        blockSize,
                                        sectorsToTrim,
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

                    if(sense)
                    {
                        DecodedSense? decSense = Sense.Decode(senseBuf);

                        // Try to workaround firmware
                        if(decSense is { ASC: 0x11, ASCQ: 0x05 } || decSense?.ASC == 0x64)
                        {
                            byte scrambledSectorsToTrim   = sectorsToTrim;
                            uint scrambledBadSectorToRead = badSectorToRead;

                            // Contrary to normal read, this must always be offset fixed, because it's data not audio
                            if(offsetBytes != 0)
                            {
                                if(offsetBytes < 0)
                                {
                                    if(scrambledBadSectorToRead == 0)
                                        scrambledBadSectorToRead = uint.MaxValue - (uint)(sectorsForOffset - 1); // -1
                                    else
                                        scrambledBadSectorToRead -= (uint)sectorsForOffset;
                                }

                                scrambledSectorsToTrim += (byte)sectorsForOffset;
                            }

                            sense = _dev.ReadCd(out cmdBuf,
                                                out _,
                                                scrambledBadSectorToRead,
                                                blockSize,
                                                scrambledSectorsToTrim,
                                                MmcSectorTypes.Cdda,
                                                false,
                                                false,
                                                false,
                                                MmcHeaderCodes.None,
                                                true,
                                                false,
                                                MmcErrorField.None,
                                                supportedSubchannel,
                                                _dev.Timeout,
                                                out double cmdDuration2);

                            cmdDuration += cmdDuration2;

                            if(!sense)
                            {
                                uint scrambledBlocksToRead = scrambledSectorsToTrim;

                                FixOffsetData(offsetBytes,
                                              sectorSize,
                                              sectorsForOffset,
                                              supportedSubchannel,
                                              ref scrambledBlocksToRead,
                                              subSize,
                                              ref cmdBuf,
                                              blockSize,
                                              false);

                                // Descramble
                                cmdBuf = Sector.Scramble(cmdBuf);

                                // Check valid sector
                                CdChecksums.CheckCdSector(cmdBuf,
                                                          out bool? correctEccP,
                                                          out bool? correctEccQ,
                                                          out bool? correctEdc);

                                // Check mode, set sense if EDC/ECC validity is not correct
                                switch(cmdBuf[15] & 0x03)
                                {
                                    case 0:

                                        for(var c = 16; c < 2352; c++)
                                        {
                                            if(cmdBuf[c] == 0x00) continue;

                                            sense = true;

                                            break;
                                        }

                                        break;
                                    case 1:
                                        sense = correctEdc != true || correctEccP != true || correctEccQ != true;

                                        break;
                                    case 2:
                                        if((cmdBuf[18] & 0x20) != 0x20)
                                        {
                                            if(correctEccP != true) sense = true;

                                            if(correctEccQ != true) sense = true;
                                        }

                                        if(correctEdc != true) sense = true;

                                        break;
                                }
                            }
                        }
                    }
                }

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
                                    badSectorToRead,
                                    blockSize,
                                    0,
                                    sectorsToTrim,
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
                                    badSectorToRead,
                                    blockSize,
                                    0,
                                    sectorsToTrim,
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
                                    badSectorToRead,
                                    blockSize,
                                    0,
                                    sectorsToTrim,
                                    _dev.Timeout,
                                    out cmdDuration);
            }
            else if(read6)
            {
                sense = _dev.Read6(out cmdBuf,
                                   out senseBuf,
                                   badSectorToRead,
                                   blockSize,
                                   sectorsToTrim,
                                   _dev.Timeout,
                                   out cmdDuration);
            }

            totalDuration += cmdDuration;

            if(sense || _dev.Error)
            {
                _errorLog?.WriteLine(badSectorToRead, _dev.Error, _dev.LastError, senseBuf);

                continue;
            }

            if(!sense && !_dev.Error)
            {
                _resume.BadBlocks.Remove(badSector);
                extents.Add(badSector);
                _mediaGraph?.PaintSectorGood(badSector);
            }

            // Because one block has been partially used to fix the offset
            if(_fixOffset && audioExtents.Contains(badSector) && offsetBytes != 0)
            {
                uint blocksToRead = sectorsToTrim;

                FixOffsetData(offsetBytes,
                              sectorSize,
                              sectorsForOffset,
                              supportedSubchannel,
                              ref blocksToRead,
                              subSize,
                              ref cmdBuf,
                              blockSize,
                              false);
            }

            if(supportedSubchannel != MmcSubchannel.None)
            {
                var data = new byte[sectorSize];
                var sub  = new byte[subSize];
                Array.Copy(cmdBuf, 0,          data, 0, sectorSize);
                Array.Copy(cmdBuf, sectorSize, sub,  0, subSize);

                if(supportsLongSectors)
                    outputOptical.WriteSectorLong(data, badSector);
                else
                    outputOptical.WriteSector(Sector.GetUserData(data), badSector);

                ulong trkStartBefore = track.StartSector;

                bool indexesChanged = Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel,
                                                                               desiredSubchannel,
                                                                               sub,
                                                                               badSector,
                                                                               1,
                                                                               subLog,
                                                                               isrcs,
                                                                               (byte)track.Sequence,
                                                                               ref mcn,
                                                                               tracks,
                                                                               subchannelExtents,
                                                                               _fixSubchannelPosition,
                                                                               outputOptical,
                                                                               _fixSubchannel,
                                                                               _fixSubchannelCrc,
                                                                               _dumpLog,
                                                                               UpdateStatus,
                                                                               smallestPregapLbaPerTrack,
                                                                               true,
                                                                               out _);

                // Set tracks and go back
                if(!indexesChanged) continue;

                outputOptical.SetTracks(tracks.ToList());

                if(track.StartSector != trkStartBefore && !_resume.BadBlocks.Contains(track.StartSector))
                {
                    _resume.BadBlocks.Add(track.StartSector);

                    goto trimStart;
                }

                b--;

                continue;
            }

            if(supportsLongSectors)
                outputOptical.WriteSectorLong(cmdBuf, badSector);
            else
                outputOptical.WriteSector(Sector.GetUserData(cmdBuf), badSector);
        }

        _trimStopwatch.Stop();
        EndProgress?.Invoke();

        UpdateStatus?.Invoke(string.Format(Localization.Core.Trimming_finished_in_0,
                                           _trimStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

        _dumpLog.WriteLine(string.Format(Localization.Core.Trimming_finished_in_0,
                                         _trimStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));
    }
}