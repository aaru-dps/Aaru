// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Cache.cs
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

using System.Linq;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Logging;
using Aaru.Decryption.DVD;
using Humanizer;
using Humanizer.Bytes;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>
    ///     Dumps data when dumping from a SCSI Block Commands compliant device,
    ///     and reads the data from the device cache
    /// </summary>
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
    /// <param name="discKey">The DVD disc key</param>
    void ReadCacheData(in ulong     blocks, in uint maxBlocksToRead, in uint blockSize, DumpHardware currentTry,
                       ExtentsULong extents, ref double currentSpeed, ref double minSpeed, ref double maxSpeed,
                       ref double   totalDuration, Reader scsiReader, MhddLog mhddLog, IbgLog ibgLog,
                       ref double   imageWriteDuration, ref bool newTrim, byte[] discKey)
    {
        ulong  sectorSpeedStart = 0;
        bool   sense;
        byte[] buffer;
        uint   blocksToRead = maxBlocksToRead;
        var    outputFormat = _outputPlugin as IWritableImage;

        InitProgress?.Invoke();

        if(scsiReader.HldtstReadRaw && _resume.NextBlock > 0)

            // The HL-DT-ST buffer is stored and read in 96-sector chunks. If we start to read at an LBA which is
            // not modulo 96, the data will not be correctly fetched. Therefore, we begin every resume read with
            // filling the buffer at a known offset.
            // TODO: This is very ugly and there probably exist a more elegant way to solve this issue.
            scsiReader.ReadBlock(out _, _resume.NextBlock - _resume.NextBlock % 96 + 1, out _, out _, out _);

        for(ulong i = _resume.NextBlock; i < blocks; i += blocksToRead)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);
                _dumpLog.WriteLine(Localization.Core.Aborted);

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

            sense         =  scsiReader.ReadBlocks(out buffer, i, blocksToRead, out double cmdDuration, out _, out _);
            totalDuration += cmdDuration;

            if(!sense && !_dev.Error)
            {
                mhddLog.Write(i, cmdDuration, blocksToRead);
                ibgLog.Write(i, currentSpeed * 1024);

                _writeStopwatch.Restart();

                byte[] tmpBuf;
                var    cmi = new byte[blocksToRead];

                for(uint j = 0; j < blocksToRead; j++)
                {
                    byte[] key = buffer.Skip((int)(2064 * j + 7)).Take(5).ToArray();

                    if(key.All(static k => k == 0))
                    {
                        outputFormat.WriteSectorTag(new byte[]
                                                    {
                                                        0, 0, 0, 0, 0
                                                    },
                                                    i + j,
                                                    SectorTagType.DvdTitleKeyDecrypted);

                        _resume.MissingTitleKeys?.Remove(i + j);

                        continue;
                    }

                    CSS.DecryptTitleKey(discKey, key, out tmpBuf);
                    outputFormat.WriteSectorTag(tmpBuf, i + j, SectorTagType.DvdTitleKeyDecrypted);
                    _resume.MissingTitleKeys?.Remove(i    + j);

                    if(_storeEncrypted) continue;

                    cmi[j] = buffer[2064 * j + 6];
                }

                // Todo: Flag in the outputFormat that a sector has been decrypted
                if(!_storeEncrypted)
                {
                    ErrorNumber errno =
                        outputFormat.ReadSectorsTag(i,
                                                    blocksToRead,
                                                    SectorTagType.DvdTitleKeyDecrypted,
                                                    out byte[] titleKey);

                    if(errno != ErrorNumber.NoError)
                    {
                        ErrorMessage?.Invoke(string.Format(Localization.Core.Error_retrieving_title_key_for_sector_0,
                                                           i));
                    }
                    else
                        buffer = CSS.DecryptSectorLong(buffer, titleKey, cmi, blocksToRead);
                }

                outputFormat.WriteSectorsLong(buffer, i, blocksToRead);

                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                extents.Add(i, blocksToRead, true);
                _mediaGraph?.PaintSectorsGood(i, blocksToRead);
            }
            else
            {
                // TODO: Reset device after X errors
                if(_stopOnError) return; // TODO: Return more cleanly

                if(i + _skip > blocks) _skip = (uint)(blocks - i);

                // Write empty data
                _writeStopwatch.Restart();
                outputFormat.WriteSectors(new byte[blockSize * _skip], i, _skip);
                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                for(ulong b = i; b < i + _skip; b++) _resume.BadBlocks.Add(b);

                mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration, _skip);

                ibgLog.Write(i, 0);
                _dumpLog.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, i);
                i       += _skip - blocksToRead;
                newTrim =  true;
            }

            _writeStopwatch.Stop();
            sectorSpeedStart  += blocksToRead;
            _resume.NextBlock =  i + blocksToRead;

            double elapsed = _speedStopwatch.Elapsed.TotalSeconds;

            if(elapsed <= 0) continue;

            currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
            sectorSpeedStart = 0;
            _speedStopwatch.Restart();
        }

        _speedStopwatch.Stop();
        _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

        EndProgress?.Invoke();
    }
}