// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Error.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
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
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decryption;
using Aaru.Decryption.DVD;
using Aaru.Devices;
using Aaru.Logging;
using DVDDecryption = Aaru.Decryption.DVD.Dump;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Retries errored data when dumping from a SCSI Block Commands compliant device</summary>
    /// <param name="currentTry">Resume information</param>
    /// <param name="extents">Correctly dump extents</param>
    /// <param name="totalDuration">Total time spent in commands</param>
    /// <param name="scsiReader">SCSI reader</param>
    /// <param name="blankExtents">Blank extents</param>
    void RetrySbcData(Reader       scsiReader, DumpHardware currentTry, ExtentsULong extents, ref double totalDuration,
                      ExtentsULong blankExtents, byte[] discKey)
    {
        var             pass              = 1;
        var             forward           = true;
        var             runningPersistent = false;
        bool            sense;
        byte[]          buffer;
        bool            recoveredError;
        Modes.ModePage? currentModePage = null;
        byte[]          md6;
        byte[]          md10;
        bool            blankCheck;
        var             newBlank     = false;
        var             outputFormat = _outputPlugin as IWritableImage;

        if(_persistent)
        {
            Modes.ModePage_01_MMC pgMmc;
            Modes.ModePage_01     pg;

            sense = _dev.ModeSense6(out buffer,
                                    out _,
                                    false,
                                    ScsiModeSensePageControl.Current,
                                    0x01,
                                    _dev.Timeout,
                                    out _);

            Modes.DecodedMode? dcMode6 = null;
            if(!sense) dcMode6         = Modes.DecodeMode6(buffer, _dev.ScsiType);

            if(sense || dcMode6 is null)
            {
                sense = _dev.ModeSense10(out buffer,
                                         out _,
                                         false,
                                         ScsiModeSensePageControl.Current,
                                         0x01,
                                         _dev.Timeout,
                                         out _);

                if(!sense)
                {
                    Modes.DecodedMode? dcMode10 = Modes.DecodeMode10(buffer, _dev.ScsiType);

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
                if(_dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
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
                else
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
            }

            if(_dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
            {
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
            }
            else
            {
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
                    Pages =
                    [
                        new Modes.ModePage
                        {
                            Page         = 0x01,
                            Subpage      = 0x00,
                            PageResponse = Modes.EncodeModePage_01(pg)
                        }
                    ]
                };

                md6  = Modes.EncodeMode6(md, _dev.ScsiType);
                md10 = Modes.EncodeMode10(md, _dev.ScsiType);
            }

            UpdateStatus?.Invoke(Localization.Core.Sending_MODE_SELECT_to_drive_return_damaged_blocks);
            sense = _dev.ModeSelect(md6, out ReadOnlySpan<byte> senseBuf, true, false, _dev.Timeout, out _);

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
    repeatRetry:
        ulong[] tmpArray = _resume.BadBlocks.ToArray();

        if(firstTry)
        {
            if(_startReverse)
            {
                tmpArray = tmpArray.Reverse().ToArray();
                forward  = false;
            }

            // Spin up... usually always fails...
            _ = scsiReader.ReadBlock(out _, tmpArray[0], out _, out _, out _);

            firstTry = false;
        }

        foreach(ulong badSector in tmpArray)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);

                break;
            }

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

            if(scsiReader.HldtstReadRaw)

                // The HL-DT-ST buffer is stored and read in 96-sector chunks. If we start to read at an LBA which is
                // not modulo 96, the data will not be correctly fetched. Therefore, we begin every recovery read with
                // filling the buffer at a known offset.
                // TODO: This is very ugly and there probably exist a more elegant way to solve this issue.
                scsiReader.ReadBlock(out _, badSector - badSector % 96 + 1, out _, out _, out _);

            sense = scsiReader.ReadBlock(out buffer,
                                         badSector,
                                         out double cmdDuration,
                                         out recoveredError,
                                         out blankCheck);

            totalDuration += cmdDuration;

            if(blankCheck)
            {
                _resume.BadBlocks.Remove(badSector);
                blankExtents.Add(badSector, badSector);
                newBlank = true;

                UpdateStatus?.Invoke(string.Format(Localization.Core.Found_blank_block_0_in_pass_1, badSector, pass));

                continue;
            }

            if(!sense && !_dev.Error || recoveredError)
            {
                _resume.BadBlocks.Remove(badSector);
                extents.Add(badSector);

                if(outputFormat is null)
                {
                    ErrorMessage?.Invoke(string.Format(Localization.Core
                                                                   .Cannot_write_retried_sector_0_no_writable_output_image,
                                                       badSector));

                    continue;
                }

                if(scsiReader.ReadBuffer3CReadRaw || scsiReader.OmniDriveReadRaw || scsiReader.HldtstReadRaw)
                {
                    if(_ngcwEnabled)
                    {
                        if(TransformNgcwLongSectors(scsiReader, buffer, badSector, 1, out SectorStatus[] statuses))
                            outputFormat.WriteSectorLong(buffer, badSector, false, statuses[0]);
                        else
                        {
                            outputFormat.WriteSectorLong(buffer, badSector, false, SectorStatus.Errored);

                            // The transform failed, so the sector was not really recovered
                            _resume.BadBlocks.Add(badSector);
                            extents.Remove(badSector);

                            continue;
                        }
                    }
                    else
                    {
                        if(scsiReader.OmniDriveReadRawBluray)
                        {
                            _resume.BadBlocks.Remove(badSector);
                            outputFormat.WriteSectorLong(buffer, badSector, false, SectorStatus.Dumped);

                            continue;
                        }

                        var cmi = new byte[1];

                        byte[] key = buffer.Skip(7).Take(5).ToArray();

                        if(key.All(static k => k == 0))
                        {
                            outputFormat.WriteSectorTag(new byte[5],
                                                        badSector,
                                                        false,
                                                        SectorTagType.DvdTitleKeyDecrypted);

                            MarkTitleKeyDumped(badSector);
                        }
                        else
                        {
                            CSS.DecryptTitleKey(discKey, key, out byte[] tmpBuf);
                            outputFormat.WriteSectorTag(tmpBuf, badSector, false, SectorTagType.DvdTitleKeyDecrypted);
                            MarkTitleKeyDumped(badSector);

                            cmi[0] = buffer[6];
                        }

                        if(!_storeEncrypted)
                        {
                            ErrorNumber errno =
                                outputFormat.ReadSectorsTag(badSector,
                                                            false,
                                                            1,
                                                            SectorTagType.DvdTitleKeyDecrypted,
                                                            out byte[] titleKey);

                            if(errno != ErrorNumber.NoError)
                            {
                                ErrorMessage?.Invoke(string.Format(Localization.Core
                                                                      .Error_retrieving_title_key_for_sector_0,
                                                                   badSector));
                            }
                            else
                                buffer = CSS.DecryptSectorLong(buffer, titleKey, cmi);
                        }

                        _resume.BadBlocks.Remove(badSector);
                        outputFormat.WriteSectorLong(buffer, badSector, false, SectorStatus.Dumped);
                    }
                }
                else
                    outputFormat.WriteSector(buffer, badSector, false, SectorStatus.Dumped);

                _mediaGraph?.PaintSectorGood(badSector);

                UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_block_0_in_pass_1,
                                                   badSector,
                                                   pass));
            }
            else if(runningPersistent)
            {
                if(outputFormat is null)
                {
                    ErrorMessage?.Invoke(string.Format(Localization.Core
                                                                   .Cannot_write_retried_sector_0_no_writable_output_image,
                                                       badSector));

                    continue;
                }

                if(scsiReader.ReadBuffer3CReadRaw || scsiReader.OmniDriveReadRaw || scsiReader.HldtstReadRaw)
                    outputFormat.WriteSectorLong(buffer, badSector, false, SectorStatus.Errored);
                else
                    outputFormat.WriteSector(buffer, badSector, false, SectorStatus.Errored);
            }
        }

        if(pass < _retryPasses && !_aborted && _resume.BadBlocks.Count > 0)
        {
            pass++;
            forward = !forward;
            _resume.BadBlocks.Sort();

            if(!forward) _resume.BadBlocks.Reverse();

            goto repeatRetry;
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

            UpdateStatus?.Invoke(Localization.Core.Sending_MODE_SELECT_to_drive_return_device_to_previous_status);
            sense = _dev.ModeSelect(md6, out _, true, false, _dev.Timeout, out _);

            if(sense) _dev.ModeSelect10(md10, out _, true, false, _dev.Timeout, out _);
        }

        if(newBlank) _resume.BlankExtents = ExtentsConverter.ToMetadata(blankExtents).ToArray();

        EndProgress?.Invoke();
    }

    void RetryTitleKeys(DVDDecryption dvdDecrypt, byte[] discKey, ref double totalDuration)
    {
        var    pass    = 1;
        var    forward = true;
        bool   sense;
        byte[] buffer;

        if(_outputPlugin is not IWritableImage outputFormat) return;

        InitProgress?.Invoke();

    repeatRetry:
        ulong[] tmpArray = MissingTitleKeysSnapshot(forward);

        foreach(ulong missingKey in tmpArray)
        {
            if(_aborted)
            {
                UpdateStatus?.Invoke(Localization.Core.Aborted);

                break;
            }

            PulseProgress?.Invoke(forward
                                      ? string.Format(Localization.Core.Retrying_title_key_0_pass_1_forward,
                                                      missingKey,
                                                      pass)
                                      : string.Format(Localization.Core.Retrying_title_key_0_pass_1_reverse,
                                                      missingKey,
                                                      pass));

            sense = dvdDecrypt.ReadTitleKey(out buffer,
                                            out _,
                                            DvdCssKeyClass.DvdCssCppmOrCprm,
                                            missingKey,
                                            _dev.Timeout,
                                            out double cmdDuration);

            totalDuration += cmdDuration;

            if(sense || _dev.Error) continue;

            CSS_CPRM.TitleKey? titleKey = CSS.DecodeTitleKey(buffer, dvdDecrypt.BusKey);

            if(!titleKey.HasValue) continue;

            outputFormat.WriteSectorTag([titleKey.Value.CMI], missingKey, false, SectorTagType.DvdSectorCmi);

            // If the CMI bit is 1, the sector is using copy protection, else it is not
            // If the decoded title key is zeroed, there should be no copy protection
            if((titleKey.Value.CMI & 0x80) >> 7 == 0 || titleKey.Value.Key.All(static k => k == 0))
            {
                outputFormat.WriteSectorTag(new byte[5], missingKey, false, SectorTagType.DvdSectorTitleKey);

                outputFormat.WriteSectorTag(new byte[5], missingKey, false, SectorTagType.DvdTitleKeyDecrypted);

                MarkTitleKeyDumped(missingKey);

                UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_title_key_0_in_pass_1,
                                                   missingKey,
                                                   pass));
            }
            else
            {
                outputFormat.WriteSectorTag(titleKey.Value.Key, missingKey, false, SectorTagType.DvdSectorTitleKey);
                MarkTitleKeyDumped(missingKey);

                if(discKey != null)
                {
                    CSS.DecryptTitleKey(discKey, titleKey.Value.Key, out buffer);
                    outputFormat.WriteSectorTag(buffer, missingKey, false, SectorTagType.DvdTitleKeyDecrypted);
                }

                UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_title_key_0_in_pass_1,
                                                   missingKey,
                                                   pass));
            }
        }

        if(pass < _retryPasses && !_aborted && MissingTitleKeyCount() > 0)
        {
            pass++;
            forward = !forward;

            goto repeatRetry;
        }

        EndProgress?.Invoke();
    }
}