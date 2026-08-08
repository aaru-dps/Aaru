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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Core.Logging;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Localization;
using Aaru.Logging;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.Core.Devices.Dumping;

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
    void RetryCdUserData(ExtentsULong audioExtents, uint blockSize, DumpHardware currentTry, ExtentsULong extents,
                         int offsetBytes, bool readcd, int sectorsForOffset, uint subSize,
                         MmcSubchannel supportedSubchannel, ref double totalDuration, SubchannelLog subLog,
                         MmcSubchannel desiredSubchannel, Track[] tracks, Dictionary<byte, string> isrcs,
                         ref string mcn, HashSet<int> subchannelExtents,
                         Dictionary<byte, int> smallestPregapLbaPerTrack, bool supportsLongSectors)
    {
        var                sense  = true;     // Sense indicator
        byte[]             cmdBuf = null;     // Data buffer
        double             cmdDuration;       // Command execution time
        const uint         sectorSize = 2352; // Full sector size
        ReadOnlySpan<byte> senseBuf   = null; // Sense buffer
        PlextorSubchannel  supportedPlextorSubchannel;
        var                outputOptical = _outputPlugin as IWritableOpticalImage;

        supportedPlextorSubchannel = supportedSubchannel switch
                                     {
                                         MmcSubchannel.None => PlextorSubchannel.None,
                                         MmcSubchannel.Raw  => PlextorSubchannel.Pack,
                                         MmcSubchannel.Q16  => PlextorSubchannel.Q16,
                                         _                  => PlextorSubchannel.None
                                     };

        if(_resume.BadBlocks.Count <= 0 || _aborted || _retryPasses <= 0) return;

        var pass              = 1;
        var forward           = true;
        var runningPersistent = false;

        Modes.ModePage? currentModePage = null;
        byte[]          md6;
        byte[]          md10;

        if(_persistent && !_omnidrive)
        {
            Modes.ModePage_01_MMC pgMmc;

            sense = _dev.ModeSense6(out cmdBuf,
                                    out _,
                                    false,
                                    ScsiModeSensePageControl.Current,
                                    0x01,
                                    _dev.Timeout,
                                    out _);

            Modes.DecodedMode? dcMode6 = null;
            if(!sense) dcMode6         = Modes.DecodeMode6(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);

            if(sense || dcMode6 is null)
            {
                sense = _dev.ModeSense10(out cmdBuf,
                                         out _,
                                         false,
                                         ScsiModeSensePageControl.Current,
                                         0x01,
                                         _dev.Timeout,
                                         out _);

                if(!sense)
                {
                    Modes.DecodedMode? dcMode10 = Modes.DecodeMode10(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);

                    if(dcMode10?.Pages != null)
                    {
                        foreach(Modes.ModePage modePage in dcMode10.Value.Pages.Where(static modePage =>
                                    modePage is { Page: 0x01, Subpage: 0x00 }))
                            currentModePage = modePage;
                    }
                }
            }
            else
            {
                if(dcMode6.Value.Pages != null)
                {
                    foreach(Modes.ModePage modePage in dcMode6.Value.Pages.Where(static modePage =>
                                modePage is { Page: 0x01, Subpage: 0x00 }))
                        currentModePage = modePage;
                }
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
                Pages =
                [
                    new Modes.ModePage
                    {
                        Page         = 0x01,
                        Subpage      = 0x00,
                        PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                    }
                ]
            };

            md6  = Modes.EncodeMode6(md, _dev.ScsiType);
            md10 = Modes.EncodeMode10(md, _dev.ScsiType);

            UpdateStatus?.Invoke(Localization.Core.Sending_MODE_SELECT_to_drive_return_damaged_blocks);
            sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

            if(sense) sense = _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);

            if(sense)
            {
                UpdateStatus?.Invoke(Localization.Core
                                                 .Drive_did_not_accept_MODE_SELECT_command_for_persistent_error_reading);

                AaruLogging.Debug(MODULE_NAME, Localization.Core.Error_0, Sense.PrettifySense(senseBuf));
            }
            else
                runningPersistent = true;
        }

        InitProgress?.Invoke();
        var firstTry = true;

    cdRepeatRetry:
        ulong[]     tmpArray              = _resume.BadBlocks.ToArray();
        List<ulong> sectorsNotEvenPartial = [];

        if(firstTry)
        {
            if(_startReverse)
            {
                tmpArray = tmpArray.Reverse().ToArray();
                forward  = false;
            }

            // Spin up... usually always fails...
            _ = _dev.ReadCd(out _,
                            out _,
                            (uint)tmpArray[0],
                            blockSize,
                            1,
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
                            out _);

            firstTry = false;
        }

        for(var i = 0; i < tmpArray.Length; i++)
        {
            ulong badSector = tmpArray[i];

            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                AaruLogging.WriteLine(Localization.Core.Aborted);

                break;
            }

            if(_skipSafedisc && badSector is >= 800 and <= 10000) continue;

            if(forward)
            {
                PulseProgress?.Invoke(runningPersistent
                                          ? string.Format(Localization.Core
                                                                      .Retrying_sector_0_pass_1_recovering_partial_data_forward,
                                                          badSector,
                                                          pass)
                                          : string.Format(Localization.Core.Retrying_sector_0_pass_1_forward,
                                                          badSector,
                                                          pass));
            }
            else
            {
                PulseProgress?.Invoke(runningPersistent
                                          ? string.Format(Localization.Core
                                                                      .Retrying_sector_0_pass_1_recovering_partial_data_reverse,
                                                          badSector,
                                                          pass)
                                          : string.Format(Localization.Core.Retrying_sector_0_pass_1_reverse,
                                                          badSector,
                                                          pass));
            }

            Track track = tracks.OrderBy(static t => t.StartSector).LastOrDefault(t => badSector >= t.StartSector);

            byte sectorsToReRead   = 1;
            var  badSectorToReRead = (uint)badSector;

            if((_fixOffset && audioExtents.Contains(badSector) || _omnidrive) && offsetBytes != 0)
            {
                if(offsetBytes < 0)
                {
                    if(badSectorToReRead == 0)
                        badSectorToReRead = uint.MaxValue - (uint)(sectorsForOffset - 1); // -1
                    else
                        badSectorToReRead -= (uint)sectorsForOffset;
                }

                sectorsToReRead += (byte)sectorsForOffset;
            }

            byte[] c2Pointers = null;

            if(_omnidrive)
            {
                bool c2Sense = _dev.OmniDriveReadCdWithC2(out byte[] c2Buf,
                                                          out senseBuf,
                                                          badSectorToReRead,
                                                          sectorsToReRead,
                                                          _dev.Timeout,
                                                          out cmdDuration,
                                                          true);

                if(!c2Sense)
                {
                    sense = false;

                    // badSectorToReRead may be < badSector when offsetBytes < 0.
                    var sectorIndex = (int)((uint)badSector - badSectorToReRead);

                    // Extract C2 for the target sector from the coherent c2Buf.
                    c2Pointers = new byte[294];
                    Array.Copy(c2Buf, sectorIndex * 2742 + 2352, c2Pointers, 0, 294);

                    // Rebuild cmdBuf in OmniDriveReadCd format (2448 per sector = data+sub)
                    // so FixOffsetData and downstream code see the expected layout.
                    cmdBuf = new byte[2448 * sectorsToReRead];

                    for(var s = 0; s < sectorsToReRead; s++)
                    {
                        int src = s * 2742;
                        int dst = s * 2448;
                        Array.Copy(c2Buf, src,        cmdBuf, dst,        2352); // data
                        Array.Copy(c2Buf, src + 2646, cmdBuf, dst + 2352, 96);   // sub
                    }
                }
                else
                {
                    sense = _dev.OmniDriveReadCd(out cmdBuf,
                                                 out senseBuf,
                                                 badSectorToReRead,
                                                 sectorsToReRead,
                                                 _dev.Timeout,
                                                 out cmdDuration,
                                                 true);
                }
            }
            else if(_supportsPlextorD8 && audioExtents.Contains(badSector))
            {
                sense = ReadPlextorWithSubchannel(out cmdBuf,
                                                  out senseBuf,
                                                  badSectorToReRead,
                                                  blockSize,
                                                  sectorsToReRead,
                                                  supportedPlextorSubchannel,
                                                  out cmdDuration);

                totalDuration += cmdDuration;
            }
            else if(readcd)
            {
                if(audioExtents.Contains(badSector))
                {
                    sense = _dev.ReadCd(out cmdBuf,
                                        out senseBuf,
                                        badSectorToReRead,
                                        blockSize,
                                        sectorsToReRead,
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
                                                badSectorToReRead,
                                                blockSize,
                                                sectorsToReRead,
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
                                        badSectorToReRead,
                                        blockSize,
                                        sectorsToReRead,
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
                            byte scrambledSectorsToReRead   = sectorsToReRead;
                            uint scrambledBadSectorToReRead = badSectorToReRead;

                            // Contrary to normal read, this must always be offset fixed, because it's data not audio
                            if(offsetBytes != 0)
                            {
                                if(offsetBytes < 0)
                                {
                                    if(scrambledBadSectorToReRead == 0)
                                        scrambledBadSectorToReRead = uint.MaxValue - (uint)(sectorsForOffset - 1); // -1
                                    else
                                        scrambledBadSectorToReRead -= (uint)sectorsForOffset;
                                }

                                scrambledSectorsToReRead += (byte)sectorsForOffset;
                            }

                            sense = _dev.ReadCd(out cmdBuf,
                                                out _,
                                                scrambledBadSectorToReRead,
                                                blockSize,
                                                scrambledSectorsToReRead,
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
                                uint scrambledBlocksToReRead = scrambledSectorsToReRead;

                                FixOffsetData(offsetBytes,
                                              sectorSize,
                                              sectorsForOffset,
                                              supportedSubchannel,
                                              ref scrambledBlocksToReRead,
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

            if(sense || _dev.Error)
            {
                _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf.ToArray());

                if(!runningPersistent) continue;

                DecodedSense? decSense = Sense.Decode(senseBuf);

                // MEDIUM ERROR, retry with ignore error below
                if(decSense is { ASC: 0x11 })
                {
                    if(!sectorsNotEvenPartial.Contains(badSector)) sectorsNotEvenPartial.Add(badSector);
                }
            }

            // Because one block has been partially used to fix the offset
            if((_fixOffset && audioExtents.Contains(badSector) || _omnidrive) && offsetBytes != 0)
            {
                uint blocksToRead = sectorsToReRead;

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

            SectorStatus sectorStatus = SectorStatus.Dumped;

            if(!sense && !_dev.Error)
            {
                if(_omnidrive)
                {
                    var sector = new byte[sectorSize];
                    Array.Copy(cmdBuf, 0, sector, 0, sectorSize);

                    if(IsScrambledData(sector, (int)badSector, out _) || !audioExtents.Contains(badSector))
                    {
                        sector = Sector.Scramble(sector);

                        SectorFixResult fixStatus = c2Pointers == null
                                                        ? CdChecksums.FixSector(sector)
                                                        : CdChecksums.FixSector(sector, c2Pointers);

                        if(fixStatus == SectorFixResult.Correct) _correctSectors++;
                        if(fixStatus == SectorFixResult.Fixed) _fixedSectors++;

                        // A NotApplicable result is only trustworthy as "nothing to fix" if the sync mark is
                        // actually present (e.g. a genuine Mode 0 or Mode 2 Form 2 sector). Otherwise it means
                        // the sector is corrupted/misaligned and must still be treated as a failure.
                        bool legitNotApplicable = fixStatus == SectorFixResult.NotApplicable &&
                                                  HasValidSync(sector)                       &&
                                                  (sector[0x00F] & 0x03) == 0x02             &&
                                                  (sector[0x012] & 0x20) == 0x20;

                        if(fixStatus == SectorFixResult.Correct ||
                           fixStatus == SectorFixResult.Fixed   ||
                           legitNotApplicable)
                        {
                            _resume.BadBlocks.Remove(badSector);
                            extents.Add(badSector);
                            _mediaGraph?.PaintSectorGood(badSector);
                            sectorsNotEvenPartial.Remove(badSector);

                            UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_sector_0_in_pass_1,
                                                               badSector,
                                                               pass));
                        }
                        else if(!audioExtents.Contains(badSector))
                        {
                            // ECC correction failed — try READ CD (drive's CIRC) as last resort.
                            bool readCdSense = _dev.ReadCd(out byte[] readCdBuf,
                                                           out _,
                                                           (uint)badSector,
                                                           sectorSize,
                                                           1,
                                                           MmcSectorTypes.AllTypes,
                                                           false,
                                                           false,
                                                           true,
                                                           MmcHeaderCodes.AllHeaders,
                                                           true,
                                                           true,
                                                           MmcErrorField.None,
                                                           MmcSubchannel.None,
                                                           _dev.Timeout,
                                                           out double readCdDuration);

                            totalDuration += readCdDuration;

                            if(!readCdSense && !_dev.Error && CdChecksums.CheckCdSector(readCdBuf) == true)
                            {
                                Array.Copy(readCdBuf, 0, sector, 0, sectorSize);
                                _resume.BadBlocks.Remove(badSector);
                                extents.Add(badSector);
                                _mediaGraph?.PaintSectorGood(badSector);
                                sectorsNotEvenPartial.Remove(badSector);

                                UpdateStatus?.Invoke(string.Format(Localization.Core
                                                                      .Correctly_retried_sector_0_in_pass_1,
                                                                   badSector,
                                                                   pass));
                            }
                        }

                        Array.Copy(sector, 0, cmdBuf, 0, sectorSize);
                    }
                    else
                    {
                        _resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        _mediaGraph?.PaintSectorGood(badSector);
                        sectorsNotEvenPartial.Remove(badSector);
                    }
                }
                else if(!audioExtents.Contains(badSector) && _paranoia)
                {
                    var sector = new byte[sectorSize];
                    Array.Copy(cmdBuf, 0, sector, 0, sectorSize);

                    // Check valid sector
                    CdChecksums.CheckCdSector(sector,
                                              out bool? correctEccP,
                                              out bool? correctEccQ,
                                              out bool? correctEdc);

                    if(correctEdc != true || correctEccP != true || correctEccQ != true)
                    {
                        if(_cureParanoia && correctEdc == true && correctEccP == true)
                        {
                            Track trk = tracks.FirstOrDefault(t => t.StartSector <= badSector &&
                                                                   t.EndSector   >= badSector);

                            SectorBuilder sb = new();
                            sb.ReconstructEcc(ref sector, track.Type);
                            Array.Copy(sector, 0, cmdBuf, 0, sectorSize);

                            _resume.BadBlocks.Remove(badSector);
                            extents.Add(badSector);
                            _mediaGraph?.PaintSectorGood(badSector);

                            UpdateStatus?.Invoke(string.Format(UI.Fixed_ECC_Q_for_sector_0, badSector));

                            sectorsNotEvenPartial.Remove(badSector);
                        }
                        else
                        {
                            sectorStatus = SectorStatus.Errored;
                            _mediaGraph?.PaintSectorBad(badSector);

                            if(correctEdc != true)
                                UpdateStatus?.Invoke(string.Format(UI.Incorrect_EDC_in_sector_0, badSector));

                            if(correctEccP != true)
                                UpdateStatus?.Invoke(string.Format(UI.Incorrect_ECC_P_in_sector_0, badSector));

                            if(correctEccQ != true)
                                UpdateStatus?.Invoke(string.Format(UI.Incorrect_ECC_Q_in_sector_0, badSector));
                        }
                    }
                    else
                    {
                        _resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        _mediaGraph?.PaintSectorGood(badSector);

                        UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_sector_0_in_pass_1,
                                                           badSector,
                                                           pass));

                        sectorsNotEvenPartial.Remove(badSector);
                    }
                }
                else
                {
                    _resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                    _mediaGraph?.PaintSectorGood(badSector);

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_sector_0_in_pass_1,
                                                       badSector,
                                                       pass));

                    sectorsNotEvenPartial.Remove(badSector);
                }
            }
            else if(_omnidrive && !audioExtents.Contains(badSector))
            {
                // Read scrambled failed, for some reason... — try READ CD as a last resort.
                bool readCdSense = _dev.ReadCd(out byte[] readCdBuf,
                                               out _,
                                               (uint)badSector,
                                               sectorSize,
                                               1,
                                               MmcSectorTypes.AllTypes,
                                               false,
                                               false,
                                               true,
                                               MmcHeaderCodes.AllHeaders,
                                               true,
                                               true,
                                               MmcErrorField.None,
                                               MmcSubchannel.None,
                                               _dev.Timeout,
                                               out double readCdDuration);

                totalDuration += readCdDuration;

                if(!readCdSense && !_dev.Error && CdChecksums.CheckCdSector(readCdBuf) == true)
                {
                    Array.Copy(readCdBuf, 0, cmdBuf, 0, sectorSize);
                    _resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                    _mediaGraph?.PaintSectorGood(badSector);
                    sectorsNotEvenPartial.Remove(badSector);

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_sector_0_in_pass_1,
                                                       badSector,
                                                       pass));
                }
            }
            else
                _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf.ToArray());

            if(supportedSubchannel != MmcSubchannel.None)
            {
                var data = new byte[sectorSize];
                var sub  = new byte[subSize];
                Array.Copy(cmdBuf, 0,          data, 0, sectorSize);
                Array.Copy(cmdBuf, sectorSize, sub,  0, subSize);

                if(supportsLongSectors)
                    outputOptical.WriteSectorLong(data, badSector, false, sectorStatus);
                else
                    outputOptical.WriteSector(Sector.GetUserData(data), badSector, false, sectorStatus);

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
                                                                               UpdateStatus,
                                                                               smallestPregapLbaPerTrack,
                                                                               true,
                                                                               out _);

                // Set tracks and go back
                if(!indexesChanged) continue;

                outputOptical.SetTracks(tracks.ToList());
                i--;
            }
            else
            {
                if(supportsLongSectors)
                    outputOptical.WriteSectorLong(cmdBuf, badSector, false, sectorStatus);
                else
                    outputOptical.WriteSector(Sector.GetUserData(cmdBuf), badSector, false, sectorStatus);
            }
        }

        if(pass < _retryPasses && !_aborted && _resume.BadBlocks.Count > 0)
        {
            pass++;
            forward = !forward;
            _resume.BadBlocks.Sort();

            if(!forward) _resume.BadBlocks.Reverse();

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
        if(_persistent && !_omnidrive && sectorsNotEvenPartial.Count > 0)
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
                Pages =
                [
                    new Modes.ModePage
                    {
                        Page         = 0x01,
                        Subpage      = 0x00,
                        PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                    }
                ]
            };

            md6  = Modes.EncodeMode6(md, _dev.ScsiType);
            md10 = Modes.EncodeMode10(md, _dev.ScsiType);

            AaruLogging.WriteLine(Localization.Core.Sending_MODE_SELECT_to_drive_ignore_error_correction);
            sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

            if(sense) sense = _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);

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
                        AaruLogging.WriteLine(Localization.Core.Aborted);

                        break;
                    }

                    PulseProgress?.Invoke(string.Format(Localization.Core.Trying_to_get_partial_data_for_sector_0,
                                                        badSector));

                    Track track = tracks.OrderBy(static t => t.StartSector)
                                        .LastOrDefault(t => badSector >= t.StartSector);

                    if(readcd)
                    {
                        sense = _dev.ReadCd(out cmdBuf,
                                            out senseBuf,
                                            (uint)badSector,
                                            blockSize,
                                            1,
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

                    if(sense || _dev.Error)
                    {
                        _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf.ToArray());

                        continue;
                    }

                    AaruLogging.WriteLine(Localization.Core.Got_partial_data_for_sector_0_in_pass_1, badSector, pass);

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        var data = new byte[sectorSize];
                        var sub  = new byte[subSize];
                        Array.Copy(cmdBuf, 0,          data, 0, sectorSize);
                        Array.Copy(cmdBuf, sectorSize, sub,  0, subSize);

                        if(supportsLongSectors)
                            outputOptical.WriteSectorLong(data, badSector, false, SectorStatus.Errored);
                        else
                            outputOptical.WriteSector(Sector.GetUserData(data), badSector, false, SectorStatus.Errored);

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
                            UpdateStatus,
                            smallestPregapLbaPerTrack,
                            true,
                            out _);

                        // Set tracks and go back
                        if(!indexesChanged) continue;

                        outputOptical.SetTracks(tracks.ToList());
                        i--;
                    }
                    else
                    {
                        if(supportsLongSectors)
                            outputOptical.WriteSectorLong(cmdBuf, badSector, false, SectorStatus.Errored);
                        else
                        {
                            outputOptical.WriteSector(Sector.GetUserData(cmdBuf),
                                                      badSector,
                                                      false,
                                                      SectorStatus.Errored);
                        }
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
                Pages  = [currentModePage.Value]
            };

            md6  = Modes.EncodeMode6(md, _dev.ScsiType);
            md10 = Modes.EncodeMode10(md, _dev.ScsiType);

            AaruLogging.WriteLine(Localization.Core.Sending_MODE_SELECT_to_drive_return_device_to_previous_status);
            sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

            if(sense) _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);
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
        var                sense  = true;   // Sense indicator
        byte[]             cmdBuf = null;   // Data buffer
        double             cmdDuration;     // Command execution time
        ReadOnlySpan<byte> senseBuf = null; // Sense buffer
        PlextorSubchannel  supportedPlextorSubchannel;
        var                outputOptical = _outputPlugin as IWritableOpticalImage;

        if(supportedSubchannel == MmcSubchannel.None || desiredSubchannel == MmcSubchannel.None) return;

        supportedPlextorSubchannel = supportedSubchannel switch
                                     {
                                         MmcSubchannel.Raw => PlextorSubchannel.All,
                                         MmcSubchannel.Q16 => PlextorSubchannel.Q16,
                                         MmcSubchannel.Rw  => PlextorSubchannel.Pack,
                                         _                 => PlextorSubchannel.None
                                     };

        if(_aborted) return;

        var pass    = 1;
        var forward = true;

        InitProgress?.Invoke();

    cdRepeatRetry:

        _resume.BadSubchannels = [];
        _resume.BadSubchannels.AddRange(subchannelExtents);
        _resume.BadSubchannels.Sort();

        if(!forward) _resume.BadSubchannels.Reverse();

        int[] tmpArray = _resume.BadSubchannels.ToArray();

        foreach(int bs in tmpArray)
        {
            var badSector = (uint)bs;

            Track track = tracks.OrderBy(static t => t.StartSector).LastOrDefault(t => badSector >= t.StartSector);

            if(_aborted)
            {
                AaruLogging.WriteLine(Localization.Core.Aborted);

                break;
            }

            PulseProgress?.Invoke(forward
                                      ? string.Format(Localization.Core.Retrying_sector_0_subchannel_pass_1_forward,
                                                      badSector,
                                                      pass)
                                      : string.Format(Localization.Core.Retrying_sector_0_subchannel_pass_1_reverse,
                                                      badSector,
                                                      pass));

            uint startSector = badSector >= 2 ? badSector - 2 : 0;

            if(_supportsPlextorD8)
            {
                sense = _dev.PlextorReadCdDa(out cmdBuf,
                                             out senseBuf,
                                             startSector,
                                             subSize,
                                             5,
                                             supportedPlextorSubchannel,
                                             0,
                                             out cmdDuration);

                totalDuration += cmdDuration;
            }
            else if(readcd)
            {
                sense = _dev.ReadCd(out cmdBuf,
                                    out senseBuf,
                                    startSector,
                                    subSize,
                                    5,
                                    track.Type == TrackType.Audio ? MmcSectorTypes.Cdda : MmcSectorTypes.AllTypes,
                                    false,
                                    false,
                                    false,
                                    MmcHeaderCodes.None,
                                    false,
                                    false,
                                    MmcErrorField.None,
                                    supportedSubchannel,
                                    _dev.Timeout,
                                    out cmdDuration);

                totalDuration += cmdDuration;
            }

            if(sense || _dev.Error)
            {
                _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf.ToArray());

                continue;
            }

            Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel,
                                                     desiredSubchannel,
                                                     cmdBuf,
                                                     startSector,
                                                     5,
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
                                                     UpdateStatus,
                                                     smallestPregapLbaPerTrack,
                                                     true,
                                                     out _);

            if(subchannelExtents.Contains(bs)) continue;

            UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_sector_0_subchannel_in_pass_1,
                                               badSector,
                                               pass));
        }

        if(pass < _retryPasses && !_aborted && subchannelExtents.Count > 0)
        {
            pass++;
            forward = !forward;

            goto cdRepeatRetry;
        }

        EndProgress?.Invoke();
    }
}