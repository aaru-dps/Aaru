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

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Logging;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Schemas;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping
{
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
        void TrimCdUserData(ExtentsULong audioExtents, uint blockSize, DumpHardwareType currentTry,
                            ExtentsULong extents, bool newTrim, int offsetBytes, bool read6, bool read10, bool read12,
                            bool read16, bool readcd, int sectorsForOffset, uint subSize,
                            MmcSubchannel supportedSubchannel, bool supportsLongSectors, ref double totalDuration,
                            SubchannelLog subLog, MmcSubchannel desiredSubchannel, Track[] tracks,
                            Dictionary<byte, string> isrcs, ref string mcn, HashSet<int> subchannelExtents,
                            Dictionary<byte, int> smallestPregapLbaPerTrack)
        {
            DateTime          start;
            DateTime          end;
            bool              sense       = true; // Sense indicator
            byte[]            cmdBuf      = null; // Data buffer
            double            cmdDuration = 0;    // Command execution time
            const uint        sectorSize  = 2352; // Full sector size
            PlextorSubchannel supportedPlextorSubchannel;
            byte[]            senseBuf = null;

            switch(supportedSubchannel)
            {
                case MmcSubchannel.None:
                    supportedPlextorSubchannel = PlextorSubchannel.None;

                    break;
                case MmcSubchannel.Raw:
                    supportedPlextorSubchannel = PlextorSubchannel.Pack;

                    break;
                case MmcSubchannel.Q16:
                    supportedPlextorSubchannel = PlextorSubchannel.Q16;

                    break;
                default:
                    supportedPlextorSubchannel = PlextorSubchannel.None;

                    break;
            }

            if(_resume.BadBlocks.Count <= 0 ||
               _aborted                     ||
               !_trim                       ||
               !newTrim)
                return;

            start = DateTime.UtcNow;
            UpdateStatus?.Invoke("Trimming skipped sectors");
            _dumpLog.WriteLine("Trimming skipped sectors");
            InitProgress?.Invoke();

            trimStart:
            ulong[] tmpArray = _resume.BadBlocks.ToArray();

            for(int b = 0; b < tmpArray.Length; b++)
            {
                ulong badSector = tmpArray[b];

                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke("Aborted!");
                    _dumpLog.WriteLine("Aborted!");

                    break;
                }

                PulseProgress?.Invoke($"Trimming sector {badSector}");

                Track track = tracks.OrderBy(t => t.TrackStartSector).
                                     LastOrDefault(t => badSector >= t.TrackStartSector);

                byte sectorsToTrim   = 1;
                uint badSectorToRead = (uint)badSector;

                if(_fixOffset                       &&
                   audioExtents.Contains(badSector) &&
                   offsetBytes != 0)
                {
                    if(offsetBytes > 0)
                    {
                        badSectorToRead -= (uint)sectorsForOffset;
                    }

                    sectorsToTrim = (byte)(sectorsForOffset + 1);
                }

                bool forceFixOffset = false;

                if(_supportsPlextorD8 && audioExtents.Contains(badSector))
                    sense = ReadPlextorWithSubchannel(out cmdBuf, out senseBuf, badSectorToRead, blockSize,
                                                      sectorsToTrim, supportedPlextorSubchannel, out cmdDuration);
                else if(readcd)
                {
                    if(audioExtents.Contains(badSector))
                    {
                        sense = _dev.ReadCd(out cmdBuf, out senseBuf, badSectorToRead, blockSize, sectorsToTrim,
                                            MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true, false,
                                            MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);

                        if(sense)
                        {
                            DecodedSense? decSense = Sense.Decode(senseBuf);

                            // Try to workaround firmware
                            if((decSense?.ASC == 0x11 && decSense?.ASCQ == 0x05) ||
                               decSense?.ASC == 0x64)
                            {
                                sense = _dev.ReadCd(out cmdBuf, out _, badSectorToRead, blockSize, sectorsToTrim,
                                                    MmcSectorTypes.AllTypes, false, false, true,
                                                    MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                                    supportedSubchannel, _dev.Timeout, out double cmdDuration2);

                                cmdDuration += cmdDuration2;
                            }
                        }
                    }
                    else
                    {
                        sense = _dev.ReadCd(out cmdBuf, out senseBuf, badSectorToRead, blockSize, sectorsToTrim,
                                            MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                            true, true, MmcErrorField.None, supportedSubchannel, _dev.Timeout,
                                            out cmdDuration);

                        if(sense)
                        {
                            DecodedSense? decSense = Sense.Decode(senseBuf);

                            // Try to workaround firmware
                            if(decSense?.ASC == 0x64)
                            {
                                sense = _dev.ReadCd(out cmdBuf, out _, badSectorToRead, blockSize, sectorsToTrim,
                                                    MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true,
                                                    false, MmcErrorField.None, supportedSubchannel, _dev.Timeout,
                                                    out double cmdDuration2);

                                cmdDuration += cmdDuration2;
                            }
                        }
                    }

                    totalDuration += cmdDuration;
                }
                else if(read16)
                    sense = _dev.Read16(out cmdBuf, out senseBuf, 0, false, true, false, badSectorToRead, blockSize, 0,
                                        sectorsToTrim, false, _dev.Timeout, out cmdDuration);
                else if(read12)
                    sense = _dev.Read12(out cmdBuf, out senseBuf, 0, false, true, false, false, badSectorToRead,
                                        blockSize, 0, sectorsToTrim, false, _dev.Timeout, out cmdDuration);
                else if(read10)
                    sense = _dev.Read10(out cmdBuf, out senseBuf, 0, false, true, false, false, badSectorToRead,
                                        blockSize, 0, sectorsToTrim, _dev.Timeout, out cmdDuration);
                else if(read6)
                    sense = _dev.Read6(out cmdBuf, out senseBuf, badSectorToRead, blockSize, sectorsToTrim,
                                       _dev.Timeout, out cmdDuration);

                totalDuration += cmdDuration;

                if(sense || _dev.Error)
                {
                    _errorLog?.WriteLine(badSectorToRead, _dev.Error, _dev.LastError, senseBuf);

                    continue;
                }

                if(!sense &&
                   !_dev.Error)
                {
                    _resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                }

                // Because one block has been partially used to fix the offset
                if(_fixOffset                       &&
                   audioExtents.Contains(badSector) &&
                   offsetBytes != 0)
                {
                    uint blocksToRead = sectorsToTrim;

                    FixOffsetData(offsetBytes, sectorSize, sectorsForOffset, supportedSubchannel, ref blocksToRead,
                                  subSize, ref cmdBuf, blockSize, false);
                }

                if(supportedSubchannel != MmcSubchannel.None)
                {
                    byte[] data = new byte[sectorSize];
                    byte[] sub  = new byte[subSize];
                    Array.Copy(cmdBuf, 0, data, 0, sectorSize);
                    Array.Copy(cmdBuf, sectorSize, sub, 0, subSize);

                    if(supportsLongSectors)
                        _outputPlugin.WriteSectorLong(data, badSector);
                    else
                        _outputPlugin.WriteSector(Sector.GetUserData(data), badSector);

                    ulong trkStartBefore = track.TrackStartSector;

                    bool indexesChanged = Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel,
                        desiredSubchannel, sub, badSector, 1, subLog, isrcs, (byte)track.TrackSequence, ref mcn,
                        tracks, subchannelExtents, _fixSubchannelPosition, _outputPlugin, _fixSubchannel,
                        _fixSubchannelCrc, _dumpLog, UpdateStatus, smallestPregapLbaPerTrack, true, out _);

                    // Set tracks and go back
                    if(!indexesChanged)
                        continue;

                    (_outputPlugin as IWritableOpticalImage).SetTracks(tracks.ToList());

                    if(track.TrackStartSector != trkStartBefore &&
                       !_resume.BadBlocks.Contains(track.TrackStartSector))
                    {
                        _resume.BadBlocks.Add(track.TrackStartSector);

                        goto trimStart;
                    }

                    b--;

                    continue;
                }

                if(supportsLongSectors)
                    _outputPlugin.WriteSectorLong(cmdBuf, badSector);
                else
                    _outputPlugin.WriteSector(Sector.GetUserData(cmdBuf), badSector);
            }

            EndProgress?.Invoke();
            end = DateTime.UtcNow;
            UpdateStatus?.Invoke($"Trimming finished in {(end - start).TotalSeconds} seconds.");
            _dumpLog.WriteLine("Trimming finished in {0} seconds.", (end - start).TotalSeconds);
        }
    }
}