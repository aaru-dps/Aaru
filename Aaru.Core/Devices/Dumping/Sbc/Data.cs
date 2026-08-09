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
using Aaru.Decoders.DVD;
using Aaru.Decryption;
using Aaru.Decryption.DVD;
using Aaru.Logging;
using Humanizer;
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
    void ReadSbcData(in ulong     blocks, in uint maxBlocksToRead, in uint blockSize, DumpHardware currentTry,
                     ExtentsULong extents, ref double currentSpeed, ref double minSpeed, ref double maxSpeed,
                     ref double   totalDuration, Reader scsiReader, MhddLog mhddLog, IbgLog ibgLog,
                     ref double   imageWriteDuration, ref bool newTrim, ref DVDDecryption dvdDecrypt, byte[] discKey)
    {
        ulong  sectorSpeedStart = 0;
        bool   sense;
        byte[] buffer;
        uint   blocksToRead = maxBlocksToRead;
        var    outputFormat = _outputPlugin as IWritableImage;

        InitProgress?.Invoke();
        _speedStopwatch.Reset();
        double elapsed = 0;

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
            sense         =  scsiReader.ReadBlocks(out buffer, i, blocksToRead, out _, out _, out _);
            elapsed       += _speedStopwatch.Elapsed.TotalMilliseconds;
            totalDuration += _speedStopwatch.Elapsed.TotalMilliseconds;
            _speedStopwatch.Stop();

            if(!sense && !_dev.Error)
            {
                if(Settings.Settings.Current.EnableDecryption && discKey != null && _titleKeys)
                {
                    for(ulong j = 0; j < blocksToRead; j++)
                    {
                        if(_aborted) break;

                        if(!ContainsMissingTitleKey(i + j))

                            // Key is already dumped.
                            continue;

                        byte[] tmpBuf;

                        bool tmpSense = dvdDecrypt.ReadTitleKey(out tmpBuf,
                                                                out _,
                                                                DvdCssKeyClass.DvdCssCppmOrCprm,
                                                                i + j,
                                                                _dev.Timeout,
                                                                out _);

                        if(tmpSense) continue;

                        CSS_CPRM.TitleKey? titleKey = CSS.DecodeTitleKey(tmpBuf, dvdDecrypt.BusKey);

                        if(titleKey.HasValue)
                            outputFormat.WriteSectorTag([titleKey.Value.CMI], i + j, false, SectorTagType.DvdSectorCmi);
                        else
                            continue;

                        // According to libdvdcss, if the key is all zeroes, the sector is actually
                        // not encrypted even if the CMI says it is.
                        if(titleKey.Value.Key.All(static k => k == 0))
                        {
                            outputFormat.WriteSectorTag(new byte[5], i + j, false, SectorTagType.DvdSectorTitleKey);

                            outputFormat.WriteSectorTag(new byte[5], i + j, false, SectorTagType.DvdTitleKeyDecrypted);

                            MarkTitleKeyDumped(i + j);

                            continue;
                        }

                        outputFormat.WriteSectorTag(titleKey.Value.Key, i + j, false, SectorTagType.DvdSectorTitleKey);
                        MarkTitleKeyDumped(i                              + j);

                        CSS.DecryptTitleKey(discKey, titleKey.Value.Key, out tmpBuf);
                        outputFormat.WriteSectorTag(tmpBuf, i + j, false, SectorTagType.DvdTitleKeyDecrypted);
                    }

                    if(!_storeEncrypted)

                        // Todo: Flag in the outputFormat that a sector has been decrypted
                    {
                        ErrorNumber errno =
                            outputFormat.ReadSectorsTag(i,
                                                        false,
                                                        blocksToRead,
                                                        SectorTagType.DvdSectorCmi,
                                                        out byte[] cmi);

                        if(errno != ErrorNumber.NoError)
                            ErrorMessage?.Invoke(string.Format(Localization.Core.Error_retrieving_CMI_for_sector_0, i));
                        else
                        {
                            errno = outputFormat.ReadSectorsTag(i,
                                                                false,
                                                                blocksToRead,
                                                                SectorTagType.DvdTitleKeyDecrypted,
                                                                out byte[] titleKey);

                            if(errno != ErrorNumber.NoError)
                            {
                                ErrorMessage?.Invoke(string.Format(Localization.Core
                                                                      .Error_retrieving_title_key_for_sector_0,
                                                                   i));
                            }
                            else
                                buffer = CSS.DecryptSector(buffer, titleKey, cmi, blocksToRead, blockSize);
                        }
                    }
                }

                mhddLog.Write(i, _speedStopwatch.Elapsed.TotalMilliseconds, blocksToRead);
                _writeStopwatch.Restart();

                outputFormat.WriteSectors(buffer,
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
                if(_dev.Manufacturer.Equals("insite", StringComparison.InvariantCultureIgnoreCase))
                {
                    _resume.BadBlocks.Add(i);
                    _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();
                    _resume.NextBlock++;
                    _abortReason = AbortReason.FatalError;
                    _aborted     = true;


                    UpdateStatus?.Invoke(Localization.Core
                                                     .INSITE_floptical_drives_get_crazy_on_the_SCSI_bus_when_an_error_is_found);

                    continue;
                }

                // TODO: Reset device after X errors
                if(_stopOnError) return; // TODO: Return more cleanly

                if(i + _skip > blocks) _skip = (uint)(blocks - i);

                // Write empty data
                _writeStopwatch.Restart();

                outputFormat.WriteSectors(new byte[blockSize * _skip],
                                          i,
                                          false,
                                          _skip,
                                          ErroredThenSkippedStatuses(blocksToRead, _skip));

                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                for(ulong b = i; b < i + _skip; b++) _resume.BadBlocks.Add(b);

                mhddLog.Write(i,
                              _speedStopwatch.Elapsed.TotalMilliseconds < 500
                                  ? 65535
                                  : _speedStopwatch.Elapsed.TotalMilliseconds,
                              _skip);

                AaruLogging.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, i);
                i       += _skip - blocksToRead;
                newTrim =  true;
            }

            _writeStopwatch.Stop();
            sectorSpeedStart  += blocksToRead;
            _resume.NextBlock =  i + blocksToRead;

            if(elapsed < 100) continue;

            currentSpeed = sectorSpeedStart * blockSize / (1048576 * elapsed / 1000);
            ibgLog.Write(i, currentSpeed                                     * 1024);
            sectorSpeedStart = 0;
            elapsed          = 0;
            _speedStopwatch.Reset();
        }

        _speedStopwatch.Stop();
        _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

        EndProgress?.Invoke();
    }
}