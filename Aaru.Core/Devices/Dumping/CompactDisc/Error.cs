// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Error.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages error recovering when dumping CompactDisc.
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
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core.Logging;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Schemas;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

partial class Dump
{
    /// <summary>Retried errored sectors in CompactDisc</summary>
    /// <param name="audioExtents">Extents with audio sectors</param>
    /// <param name="blockSize">Size of the read sector in bytes</param>
    /// <param name="currentTry">Current dump hardware try</param>
    /// <param name="extents">Extents</param>
    /// <param name="offsetBytes">Read offset</param>
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
    void RetryCdUserData(ExtentsULong audioExtents, uint blockSize, DumpHardwareType currentTry, ExtentsULong extents,
                         int offsetBytes, bool readcd, int sectorsForOffset, uint subSize,
                         MmcSubchannel supportedSubchannel, ref double totalDuration, SubchannelLog subLog,
                         MmcSubchannel desiredSubchannel, Track[] tracks, Dictionary<byte, string> isrcs,
                         ref string mcn, HashSet<int> subchannelExtents,
                         Dictionary<byte, int> smallestPregapLbaPerTrack, bool supportsLongSectors)
    {
        var               sense  = true;     // Sense indicator
        byte[]            cmdBuf = null;     // Data buffer
        double            cmdDuration;       // Command execution time
        const uint        sectorSize = 2352; // Full sector size
        byte[]            senseBuf   = null; // Sense buffer
        PlextorSubchannel supportedPlextorSubchannel;
        var               outputOptical = _outputPlugin as IWritableOpticalImage;

        supportedPlextorSubchannel = supportedSubchannel switch
                                     {
                                         MmcSubchannel.None => PlextorSubchannel.None,
                                         MmcSubchannel.Raw  => PlextorSubchannel.Pack,
                                         MmcSubchannel.Q16  => PlextorSubchannel.Q16,
                                         _                  => PlextorSubchannel.None
                                     };

        if(_resume.BadBlocks.Count <= 0 ||
           _aborted                     ||
           _retryPasses <= 0)
            return;

        var pass              = 1;
        var forward           = true;
        var runningPersistent = false;

        Modes.ModePage? currentModePage = null;
        byte[]          md6;
        byte[]          md10;

        if(_persistent)
        {
            Modes.ModePage_01_MMC pgMmc;

            sense = _dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x01, _dev.Timeout,
                                    out _);

            if(sense)
            {
                sense = _dev.ModeSense10(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x01, _dev.Timeout,
                                         out _);

                if(!sense)
                {
                    Modes.DecodedMode? dcMode10 = Modes.DecodeMode10(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);

                    if(dcMode10?.Pages != null)
                        foreach(Modes.ModePage modePage in dcMode10.Value.Pages.Where(modePage =>
                                                                                    modePage.Page == 0x01 && modePage.Subpage == 0x00))
                            currentModePage = modePage;
                }
            }
            else
            {
                Modes.DecodedMode? dcMode6 = Modes.DecodeMode6(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);

                if(dcMode6?.Pages != null)
                    foreach(Modes.ModePage modePage in dcMode6.Value.Pages.Where(modePage => modePage.Page == 0x01 &&
                                                                               modePage.Subpage                                                           == 0x00))
                        currentModePage = modePage;
            }

            if(currentModePage == null)
            {
                pgMmc = new Modes.ModePage_01_MMC
                {
                    PS             = false,
                    ReadRetryCount = 32,
                    Parameter      = 0x00
                };

                currentModePage = new Modes.ModePage
                {
                    Page         = 0x01,
                    Subpage      = 0x00,
                    PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                };
            }

            pgMmc = new Modes.ModePage_01_MMC
            {
                PS             = false,
                ReadRetryCount = 255,
                Parameter      = 0x20
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
                        PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                    }
                }
            };

            md6  = Modes.EncodeMode6(md, _dev.ScsiType);
            md10 = Modes.EncodeMode10(md, _dev.ScsiType);

            UpdateStatus?.Invoke("Sending MODE SELECT to drive (return damaged blocks).");
            _dumpLog.WriteLine("Sending MODE SELECT to drive (return damaged blocks).");
            sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

            if(sense)
                sense = _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);

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
    cdRepeatRetry:
        ulong[] tmpArray              = _resume.BadBlocks.ToArray();
        var     sectorsNotEvenPartial = new List<ulong>();

        for(var i = 0; i < tmpArray.Length; i++)
        {
            ulong badSector = tmpArray[i];

            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                _dumpLog.WriteLine("Aborted!");

                break;
            }

            PulseProgress?.Invoke(string.Format("Retrying sector {0}, pass {1}, {3}{2}", badSector, pass,
                                                forward ? "forward" : "reverse",
                                                runningPersistent ? "recovering partial data, " : ""));

            Track track = tracks.OrderBy(t => t.StartSector).LastOrDefault(t => badSector >= t.StartSector);

            byte sectorsToReRead   = 1;
            var  badSectorToReRead = (uint)badSector;

            if(_fixOffset                       &&
               audioExtents.Contains(badSector) &&
               offsetBytes != 0)
            {
                if(offsetBytes > 0)
                    badSectorToReRead -= (uint)sectorsForOffset;

                sectorsToReRead = (byte)(sectorsForOffset + 1);
            }

            if(_supportsPlextorD8 && audioExtents.Contains(badSector))
            {
                sense = ReadPlextorWithSubchannel(out cmdBuf, out senseBuf, badSectorToReRead, blockSize,
                                                  sectorsToReRead, supportedPlextorSubchannel, out cmdDuration);

                totalDuration += cmdDuration;
            }
            else if(readcd)
            {
                if(audioExtents.Contains(badSector))
                {
                    sense = _dev.ReadCd(out cmdBuf, out senseBuf, badSectorToReRead, blockSize, sectorsToReRead,
                                        MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true, false,
                                        MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);

                    if(sense)
                    {
                        DecodedSense? decSense = Sense.Decode(senseBuf);

                        // Try to workaround firmware
                        if(decSense?.ASC == 0x11 && decSense?.ASCQ == 0x05 ||
                           decSense?.ASC == 0x64)
                        {
                            sense = _dev.ReadCd(out cmdBuf, out _, badSectorToReRead, blockSize, sectorsToReRead,
                                                MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                                true, true, MmcErrorField.None, supportedSubchannel, _dev.Timeout,
                                                out double cmdDuration2);

                            cmdDuration += cmdDuration2;
                        }
                    }
                }
                else
                {
                    sense = _dev.ReadCd(out cmdBuf, out senseBuf, badSectorToReRead, blockSize, sectorsToReRead,
                                        MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                        true, MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);

                    if(sense)
                    {
                        DecodedSense? decSense = Sense.Decode(senseBuf);

                        // Try to workaround firmware
                        if(decSense?.ASC == 0x11 && decSense?.ASCQ == 0x05 ||
                           decSense?.ASC == 0x64)
                        {
                            sense = _dev.ReadCd(out cmdBuf, out _, badSectorToReRead, blockSize, sectorsToReRead,
                                                MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true,
                                                false, MmcErrorField.None, supportedSubchannel, _dev.Timeout,
                                                out double cmdDuration2);

                            cmdDuration += cmdDuration2;
                        }
                    }
                }

                totalDuration += cmdDuration;
            }

            if(sense || _dev.Error)
            {
                _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf);

                if(!runningPersistent)
                    continue;

                DecodedSense? decSense = Sense.Decode(senseBuf);

                // MEDIUM ERROR, retry with ignore error below
                if(decSense is { ASC: 0x11 })
                    if(!sectorsNotEvenPartial.Contains(badSector))
                        sectorsNotEvenPartial.Add(badSector);
            }

            // Because one block has been partially used to fix the offset
            if(_fixOffset                       &&
               audioExtents.Contains(badSector) &&
               offsetBytes != 0)
            {
                uint blocksToRead = sectorsToReRead;

                FixOffsetData(offsetBytes, sectorSize, sectorsForOffset, supportedSubchannel, ref blocksToRead, subSize,
                              ref cmdBuf, blockSize, false);
            }

            if(!sense &&
               !_dev.Error)
            {
                _resume.BadBlocks.Remove(badSector);
                extents.Add(badSector);
                UpdateStatus?.Invoke($"Correctly retried sector {badSector} in pass {pass}.");
                _dumpLog.WriteLine("Correctly retried sector {0} in pass {1}.", badSector, pass);
                sectorsNotEvenPartial.Remove(badSector);
            }
            else
                _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf);

            if(supportedSubchannel != MmcSubchannel.None)
            {
                var data = new byte[sectorSize];
                var sub  = new byte[subSize];
                Array.Copy(cmdBuf, 0, data, 0, sectorSize);
                Array.Copy(cmdBuf, sectorSize, sub, 0, subSize);

                if(supportsLongSectors)
                    outputOptical.WriteSectorLong(data, badSector);
                else
                    outputOptical.WriteSector(Sector.GetUserData(data), badSector);

                bool indexesChanged = Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel, desiredSubchannel,
                                                                               sub, badSector, 1, subLog, isrcs,
                                                                               (byte)track.Sequence, ref mcn, tracks,
                                                                               subchannelExtents,
                                                                               _fixSubchannelPosition, outputOptical,
                                                                               _fixSubchannel, _fixSubchannelCrc,
                                                                               _dumpLog, UpdateStatus,
                                                                               smallestPregapLbaPerTrack, true, out _);

                // Set tracks and go back
                if(!indexesChanged)
                    continue;

                outputOptical.SetTracks(tracks.ToList());
                i--;
            }
            else
            {
                if(supportsLongSectors)
                    outputOptical.WriteSectorLong(cmdBuf, badSector);
                else
                    outputOptical.WriteSector(Sector.GetUserData(cmdBuf), badSector);
            }
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

            goto cdRepeatRetry;
        }

        EndProgress?.Invoke();

        // TODO: Enable when underlying images support lead-outs
        /*
            RetryCdLeadOuts(blocks, blockSize, ref currentSpeed, currentTry, extents, ibgLog, ref imageWriteDuration,
                   leadOutExtents, ref maxSpeed, mhddLog, ref minSpeed, read6, read10, read12, read16, readcd,
                   supportedSubchannel, subSize, ref totalDuration);
            */

        // Try to ignore read errors, on some drives this allows to recover partial even if damaged data
        if(_persistent && sectorsNotEvenPartial.Count > 0)
        {
            var pgMmc = new Modes.ModePage_01_MMC
            {
                PS             = false,
                ReadRetryCount = 255,
                Parameter      = 0x01
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
                        PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                    }
                }
            };

            md6  = Modes.EncodeMode6(md, _dev.ScsiType);
            md10 = Modes.EncodeMode10(md, _dev.ScsiType);

            _dumpLog.WriteLine("Sending MODE SELECT to drive (ignore error correction).");
            sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

            if(sense)
                sense = _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);

            if(!sense)
            {
                runningPersistent = true;

                InitProgress?.Invoke();

                for(var i = 0; i < sectorsNotEvenPartial.Count; i++)
                {
                    ulong badSector = sectorsNotEvenPartial[i];

                    if(_aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        _dumpLog.WriteLine("Aborted!");

                        break;
                    }

                    PulseProgress?.Invoke($"Trying to get partial data for sector {badSector}");

                    Track track = tracks.OrderBy(t => t.StartSector).LastOrDefault(t => badSector >= t.StartSector);

                    if(readcd)
                    {
                        sense = _dev.ReadCd(out cmdBuf, out senseBuf, (uint)badSector, blockSize, 1,
                                            MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                            true, true, MmcErrorField.None, supportedSubchannel, _dev.Timeout,
                                            out cmdDuration);

                        totalDuration += cmdDuration;
                    }

                    if(sense || _dev.Error)
                    {
                        _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf);

                        continue;
                    }

                    _dumpLog.WriteLine("Got partial data for sector {0} in pass {1}.", badSector, pass);

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        var data = new byte[sectorSize];
                        var sub  = new byte[subSize];
                        Array.Copy(cmdBuf, 0, data, 0, sectorSize);
                        Array.Copy(cmdBuf, sectorSize, sub, 0, subSize);

                        if(supportsLongSectors)
                            outputOptical.WriteSectorLong(data, badSector);
                        else
                            outputOptical.WriteSector(Sector.GetUserData(data), badSector);

                        bool indexesChanged = Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel,
                            desiredSubchannel, sub, badSector, 1, subLog, isrcs, (byte)track.Sequence, ref mcn,
                            tracks, subchannelExtents, _fixSubchannelPosition, outputOptical, _fixSubchannel,
                            _fixSubchannelCrc, _dumpLog, UpdateStatus, smallestPregapLbaPerTrack, true, out _);

                        // Set tracks and go back
                        if(!indexesChanged)
                            continue;

                        outputOptical.SetTracks(tracks.ToList());
                        i--;
                    }
                    else
                    {
                        if(supportsLongSectors)
                            outputOptical.WriteSectorLong(cmdBuf, badSector);
                        else
                            outputOptical.WriteSector(Sector.GetUserData(cmdBuf), badSector);
                    }
                }

                EndProgress?.Invoke();
            }
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

            md6  = Modes.EncodeMode6(md, _dev.ScsiType);
            md10 = Modes.EncodeMode10(md, _dev.ScsiType);

            _dumpLog.WriteLine("Sending MODE SELECT to drive (return device to previous status).");
            sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

            if(sense)
                _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);
        }

        EndProgress?.Invoke();
    }

    /// <summary>Retried errored subchannels in CompactDisc</summary>
    /// <param name="readcd">Device supports READ CD</param>
    /// <param name="subSize">Subchannel size in bytes</param>
    /// <param name="supportedSubchannel">Drive's maximum supported subchannel</param>
    /// <param name="totalDuration">Total commands duration</param>
    /// <param name="tracks">Disc tracks</param>
    /// <param name="subLog">Subchannel log</param>
    /// <param name="desiredSubchannel">Subchannel desired to save</param>
    /// <param name="isrcs">List of disc ISRCs</param>
    /// <param name="mcn">Disc media catalogue number</param>
    /// <param name="subchannelExtents">List of subchannels not yet dumped correctly</param>
    /// <param name="smallestPregapLbaPerTrack">List of smallest pregap relative address per track</param>
    void RetrySubchannel(bool readcd, uint subSize, MmcSubchannel supportedSubchannel, ref double totalDuration,
                         SubchannelLog subLog, MmcSubchannel desiredSubchannel, Track[] tracks,
                         Dictionary<byte, string> isrcs, ref string mcn, HashSet<int> subchannelExtents,
                         Dictionary<byte, int> smallestPregapLbaPerTrack)
    {
        var               sense  = true;   // Sense indicator
        byte[]            cmdBuf = null;   // Data buffer
        double            cmdDuration;     // Command execution time
        byte[]            senseBuf = null; // Sense buffer
        PlextorSubchannel supportedPlextorSubchannel;
        var               outputOptical = _outputPlugin as IWritableOpticalImage;

        if(supportedSubchannel == MmcSubchannel.None ||
           desiredSubchannel   == MmcSubchannel.None)
            return;

        supportedPlextorSubchannel = supportedSubchannel switch
                                     {
                                         MmcSubchannel.None => PlextorSubchannel.None,
                                         MmcSubchannel.Raw  => PlextorSubchannel.All,
                                         MmcSubchannel.Q16  => PlextorSubchannel.Q16,
                                         MmcSubchannel.Rw   => PlextorSubchannel.Pack,
                                         _                  => PlextorSubchannel.None
                                     };

        if(_aborted)
            return;

        var pass    = 1;
        var forward = true;

        InitProgress?.Invoke();

    cdRepeatRetry:

        _resume.BadSubchannels = new List<int>();
        _resume.BadSubchannels.AddRange(subchannelExtents);
        _resume.BadSubchannels.Sort();

        if(!forward)
            _resume.BadSubchannels.Reverse();

        int[] tmpArray = _resume.BadSubchannels.ToArray();

        foreach(int bs in tmpArray)
        {
            var badSector = (uint)bs;

            Track track = tracks.OrderBy(t => t.StartSector).LastOrDefault(t => badSector >= t.StartSector);

            if(_aborted)
            {
                _dumpLog.WriteLine("Aborted!");

                break;
            }

            PulseProgress?.Invoke($"Retrying sector {badSector} subchannel, pass {pass}, {
                (forward ? "forward" : "reverse")}");

            uint startSector = badSector - 2;

            if(_supportsPlextorD8)
            {
                sense = _dev.PlextorReadCdDa(out cmdBuf, out senseBuf, startSector, subSize, 5,
                                             supportedPlextorSubchannel, 0, out cmdDuration);

                totalDuration += cmdDuration;
            }
            else if(readcd)
            {
                sense = _dev.ReadCd(out cmdBuf, out senseBuf, startSector, subSize, 5,
                                    track.Type == TrackType.Audio ? MmcSectorTypes.Cdda : MmcSectorTypes.AllTypes,
                                    false, false, false, MmcHeaderCodes.None, false, false, MmcErrorField.None,
                                    supportedSubchannel, _dev.Timeout, out cmdDuration);

                totalDuration += cmdDuration;
            }

            if(sense || _dev.Error)
            {
                _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf);

                continue;
            }

            Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel, desiredSubchannel, cmdBuf, badSector, 5,
                                                     subLog, isrcs, (byte)track.Sequence, ref mcn, tracks,
                                                     subchannelExtents, _fixSubchannelPosition, outputOptical,
                                                     _fixSubchannel, _fixSubchannelCrc, _dumpLog, UpdateStatus,
                                                     smallestPregapLbaPerTrack, true, out _);

            if(subchannelExtents.Contains(bs))
                continue;

            UpdateStatus?.Invoke($"Correctly retried sector {badSector} subchannel in pass {pass}.");
            _dumpLog.WriteLine("Correctly retried sector {0} subchannel in pass {1}.", badSector, pass);
        }

        if(pass < _retryPasses &&
           !_aborted           &&
           subchannelExtents.Count > 0)
        {
            pass++;
            forward = !forward;

            goto cdRepeatRetry;
        }

        EndProgress?.Invoke();
    }
}