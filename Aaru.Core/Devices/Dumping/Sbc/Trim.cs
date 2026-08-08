// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Trim.cs
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
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System.Linq;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Decryption.DVD;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Trims data when dumping from a SCSI Block Commands compliant device</summary>
    /// <param name="scsiReader">SCSI reader</param>
    /// <param name="extents">Correctly dump extents</param>
    /// <param name="currentTry">Resume information</param>
    /// <param name="blankExtents">Blank extents</param>
    void TrimSbcData(Reader scsiReader, ExtentsULong extents, DumpHardware currentTry, ExtentsULong blankExtents,
                     byte[] discKey)
    {
        ulong[] tmpArray = _resume.BadBlocks.ToArray();
        bool    sense;
        bool    recoveredError;
        bool    blankCheck;
        byte[]  buffer;
        var     newBlank = false;

        if(_outputPlugin is not IWritableImage outputFormat)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Image_is_not_writable_aborting);

            return;
        }

        foreach(ulong badSector in tmpArray)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);

                break;
            }

            PulseProgress?.Invoke(string.Format(Localization.Core.Trimming_sector_0, badSector));

            sense = scsiReader.ReadBlock(out buffer, badSector, out double _, out recoveredError, out blankCheck);

            if(blankCheck)
            {
                blankExtents.Add(badSector, badSector);
                newBlank = true;
                _resume.BadBlocks.Remove(badSector);

                UpdateStatus?.Invoke(string.Format(Localization.Core.Found_blank_block_0, badSector));

                continue;
            }

            if((sense || _dev.Error) && !recoveredError) continue;

            if(scsiReader.HldtstReadRaw)

                // The HL-DT-ST buffer is stored and read in 96-sector chunks. If we start to read at an LBA which is
                // not modulo 96, the data will not be correctly fetched. Therefore, we begin every recovery read with
                // filling the buffer at a known offset.
                // TODO: This is very ugly and there probably exist a more elegant way to solve this issue.
                scsiReader.ReadBlock(out _, badSector - badSector % 96 + 1, out _, out _, out _);

            _resume.BadBlocks.Remove(badSector);
            extents.Add(badSector);

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
                        outputFormat.WriteSectorTag(new byte[5], badSector, false, SectorTagType.DvdTitleKeyDecrypted);

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
        }

        if(newBlank) _resume.BlankExtents = ExtentsConverter.ToMetadata(blankExtents).ToArray();
    }
}