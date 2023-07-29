// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Data.cs
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
// Copyright © 2011-2023 Natalia Portillo
// Copyright © 2020-2023 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Linq;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Logging;
using Aaru.Decoders.DVD;
using Aaru.Decryption;
using Aaru.Decryption.DVD;
using DVDDecryption = Aaru.Decryption.DVD.Dump;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Dumps data when dumping from a SCSI Block Commands compliant device</summary>
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
    /// <param name="dvdDecrypt">DVD CSS decryption module</param>
    /// <param name="discKey">The DVD disc key</param>
    void ReadSbcData(in ulong blocks, in uint maxBlocksToRead, in uint blockSize, DumpHardware currentTry,
                     ExtentsULong extents, ref double currentSpeed, ref double minSpeed, ref double maxSpeed,
                     ref double totalDuration, Reader scsiReader, MhddLog mhddLog, IbgLog ibgLog,
                     ref double imageWriteDuration, ref bool newTrim, ref DVDDecryption dvdDecrypt, byte[] discKey)
    {
        ulong    sectorSpeedStart = 0;
        bool     sense;
        DateTime timeSpeedStart = DateTime.UtcNow;
        byte[]   buffer;
        uint     blocksToRead = maxBlocksToRead;
        var      outputFormat = _outputPlugin as IWritableImage;

        InitProgress?.Invoke();

        for(ulong i = _resume.NextBlock; i < blocks; i += blocksToRead)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);
                _dumpLog.WriteLine(Localization.Core.Aborted);

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

            UpdateProgress?.
                Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2_MiB_sec, i, blocks, currentSpeed),
                       (long)i, (long)blocks);

            sense         =  scsiReader.ReadBlocks(out buffer, i, blocksToRead, out double cmdDuration, out _, out _);
            totalDuration += cmdDuration;

            if(!sense &&
               !_dev.Error)
            {
                if(Settings.Settings.Current.EnableDecryption &&
                   discKey != null                            &&
                   _titleKeys)
                {
                    for(ulong j = 0; j < blocksToRead; j++)
                    {
                        if(_aborted)
                            break;

                        if(!_resume.MissingTitleKeys.Contains(i + j))

                            // Key is already dumped.
                            continue;

                        byte[] tmpBuf;

                        bool tmpSense = dvdDecrypt.ReadTitleKey(out tmpBuf, out _, DvdCssKeyClass.DvdCssCppmOrCprm,
                                                                i + j, _dev.Timeout, out _);

                        if(tmpSense)
                            continue;

                        CSS_CPRM.TitleKey? titleKey = CSS.DecodeTitleKey(tmpBuf, dvdDecrypt.BusKey);

                        if(titleKey.HasValue)
                            outputFormat.WriteSectorTag(new[]
                            {
                                titleKey.Value.CMI
                            }, i + j, SectorTagType.DvdCmi);
                        else
                            continue;

                        // If the CMI bit is 1, the sector is using copy protection, else it is not
                        if((titleKey.Value.CMI & 0x80) >> 7 == 0)
                        {
                            // The CMI indicates this sector is not encrypted.
                            outputFormat.WriteSectorTag(new byte[]
                            {
                                0, 0, 0, 0, 0
                            }, i + j, SectorTagType.DvdTitleKey);

                            outputFormat.WriteSectorTag(new byte[]
                            {
                                0, 0, 0, 0, 0
                            }, i + j, SectorTagType.DvdTitleKeyDecrypted);

                            _resume.MissingTitleKeys.Remove(i + j);

                            continue;
                        }

                        // According to libdvdcss, if the key is all zeroes, the sector is actually
                        // not encrypted even if the CMI says it is.
                        if(titleKey.Value.Key.All(k => k == 0))
                        {
                            outputFormat.WriteSectorTag(new byte[]
                            {
                                0, 0, 0, 0, 0
                            }, i + j, SectorTagType.DvdTitleKey);

                            outputFormat.WriteSectorTag(new byte[]
                            {
                                0, 0, 0, 0, 0
                            }, i + j, SectorTagType.DvdTitleKeyDecrypted);

                            _resume.MissingTitleKeys.Remove(i + j);

                            continue;
                        }

                        outputFormat.WriteSectorTag(titleKey.Value.Key, i + j, SectorTagType.DvdTitleKey);
                        _resume.MissingTitleKeys.Remove(i                 + j);

                        CSS.DecryptTitleKey(discKey, titleKey.Value.Key, out tmpBuf);
                        outputFormat.WriteSectorTag(tmpBuf, i + j, SectorTagType.DvdTitleKeyDecrypted);
                    }

                    if(!_storeEncrypted)

                        // Todo: Flag in the outputFormat that a sector has been decrypted
                    {
                        ErrorNumber errno =
                            outputFormat.ReadSectorsTag(i, blocksToRead, SectorTagType.DvdCmi, out byte[] cmi);

                        if(errno != ErrorNumber.NoError)
                            ErrorMessage?.Invoke(string.Format(Localization.Core.Error_retrieving_CMI_for_sector_0, i));
                        else
                        {
                            errno = outputFormat.ReadSectorsTag(i, blocksToRead, SectorTagType.DvdTitleKeyDecrypted,
                                                                out byte[] titleKey);

                            if(errno != ErrorNumber.NoError)
                                ErrorMessage?.
                                    Invoke(string.Format(Localization.Core.Error_retrieving_title_key_for_sector_0, i));
                            else
                                buffer = CSS.DecryptSector(buffer, titleKey, cmi, blocksToRead, blockSize);
                        }
                    }
                }

                mhddLog.Write(i, cmdDuration, blocksToRead);
                ibgLog.Write(i, currentSpeed * 1024);
                DateTime writeStart = DateTime.Now;
                outputFormat.WriteSectors(buffer, i, blocksToRead);
                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                extents.Add(i, blocksToRead, true);
                _mediaGraph?.PaintSectorsGood(i, blocksToRead);
            }
            else
            {
                if(_dev.Manufacturer.ToLowerInvariant() == "insite")
                {
                    _resume.BadBlocks.Add(i);
                    _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();
                    _resume.NextBlock++;
                    _aborted = true;

                    _dumpLog?.WriteLine(Localization.Core.
                                                     INSITE_floptical_drives_get_crazy_on_the_SCSI_bus_when_an_error_is_found);

                    UpdateStatus?.Invoke(Localization.Core.
                                                      INSITE_floptical_drives_get_crazy_on_the_SCSI_bus_when_an_error_is_found);

                    continue;
                }

                // TODO: Reset device after X errors
                if(_stopOnError)
                    return; // TODO: Return more cleanly

                if(i + _skip > blocks)
                    _skip = (uint)(blocks - i);

                // Write empty data
                DateTime writeStart = DateTime.Now;
                outputFormat.WriteSectors(new byte[blockSize * _skip], i, _skip);
                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                for(ulong b = i; b < i + _skip; b++)
                    _resume.BadBlocks.Add(b);

                mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration, _skip);

                ibgLog.Write(i, 0);
                _dumpLog.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, i);
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

        EndProgress?.Invoke();
    }
}