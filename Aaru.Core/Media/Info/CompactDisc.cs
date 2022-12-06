// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CompactDisc.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core.
//
// --[ Description ] ----------------------------------------------------------
//
//     Retrieves information from CompactDisc media.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Logging;
using Aaru.Core.Media.Detection;
using Aaru.Database.Models;
using Aaru.Decoders.CD;
using Aaru.Devices;
using Device = Aaru.Database.Models.Device;

namespace Aaru.Core.Media.Info
{
    /// <summary>Core operations for retrieving information about CD based media</summary>
    public static class CompactDisc
    {
        /// <summary>Gets the offset bytes from a Compact Disc</summary>
        /// <param name="cdOffset">Offset entry from database</param>
        /// <param name="dbDev">Device entry from database</param>
        /// <param name="debug">Debug</param>
        /// <param name="dev">Opened device</param>
        /// <param name="dskType">Detected disk type</param>
        /// <param name="dumpLog">Dump log if applicable</param>
        /// <param name="tracks">Disc track list</param>
        /// <param name="updateStatus">UpdateStatus event</param>
        /// <param name="driveOffset">Drive offset</param>
        /// <param name="combinedOffset">Combined offset</param>
        /// <param name="supportsPlextorReadCdDa">Set to <c>true</c> if drive supports PLEXTOR READ CD-DA vendor command</param>
        /// <returns><c>true</c> if offset could be found, <c>false</c> otherwise</returns>
        [SuppressMessage("ReSharper", "TooWideLocalVariableScope")]
        public static void GetOffset(CdOffset cdOffset, Device dbDev, bool debug, Aaru.Devices.Device dev,
                                     MediaType dskType, DumpLog dumpLog, Track[] tracks,
                                     UpdateStatusHandler updateStatus, out int? driveOffset, out int? combinedOffset,
                                     out bool supportsPlextorReadCdDa)
        {
            byte[]     cmdBuf;
            bool       sense;
            int        minute;
            int        second;
            int        frame;
            byte[]     sectorSync;
            byte[]     tmpBuf;
            int        lba;
            int        diff;
            Track      dataTrack   = default;
            Track      audioTrack  = default;
            bool       offsetFound = false;
            const uint sectorSize  = 2352;
            driveOffset             = cdOffset?.Offset * 4;
            combinedOffset          = null;
            supportsPlextorReadCdDa = false;

            if(dskType != MediaType.VideoNowColor)
            {
                if(tracks.Any(t => t.TrackType != TrackType.Audio))
                {
                    dataTrack = tracks.FirstOrDefault(t => t.TrackType != TrackType.Audio);

                    if(dataTrack != null)
                    {
                        // Build sync
                        sectorSync = new byte[]
                        {
                            0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00
                        };

                        tmpBuf = new byte[sectorSync.Length];

                        // Ensure to be out of the pregap, or multi-session discs give funny values
                        uint wantedLba = (uint)(dataTrack.TrackStartSector + 151);

                        // Plextor READ CDDA
                        if(dbDev?.ATAPI?.RemovableMedias?.Any(d => d.SupportsPlextorReadCDDA == true) == true ||
                           dbDev?.SCSI?.RemovableMedias?.Any(d => d.SupportsPlextorReadCDDA  == true) == true ||
                           dev.Manufacturer.ToLowerInvariant()                                        == "plextor")
                        {
                            sense = dev.PlextorReadCdDa(out cmdBuf, out _, wantedLba, sectorSize, 3,
                                                        PlextorSubchannel.None, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                supportsPlextorReadCdDa = true;

                                for(int i = 0; i < cmdBuf.Length - sectorSync.Length; i++)
                                {
                                    Array.Copy(cmdBuf, i, tmpBuf, 0, sectorSync.Length);

                                    if(!tmpBuf.SequenceEqual(sectorSync))
                                        continue;

                                    // De-scramble M and S
                                    minute = cmdBuf[i + 12] ^ 0x01;
                                    second = cmdBuf[i + 13] ^ 0x80;
                                    frame  = cmdBuf[i + 14];

                                    // Convert to binary
                                    minute = (minute / 16 * 10) + (minute & 0x0F);
                                    second = (second / 16 * 10) + (second & 0x0F);
                                    frame  = (frame  / 16 * 10) + (frame  & 0x0F);

                                    // Calculate the first found LBA
                                    lba = (minute * 60 * 75) + (second * 75) + frame - 150;

                                    // Calculate the difference between the found LBA and the requested one
                                    diff = (int)wantedLba - lba;

                                    combinedOffset = i + (2352 * diff);
                                    offsetFound    = true;

                                    break;
                                }
                            }
                        }

                        if(!offsetFound &&
                           (debug || dbDev?.ATAPI?.RemovableMedias?.Any(d => d.CanReadCdScrambled == true) == true ||
                            dbDev?.SCSI?.RemovableMedias?.Any(d => d.CanReadCdScrambled           == true) == true ||
                            dbDev?.SCSI?.MultiMediaDevice?.TestedMedia?.Any(d => d.CanReadCdScrambled == true) ==
                            true || dev.Manufacturer.ToLowerInvariant() == "hl-dt-st"))
                        {
                            sense = dev.ReadCd(out cmdBuf, out _, wantedLba, sectorSize, 3, MmcSectorTypes.Cdda, false,
                                               false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                               MmcSubchannel.None, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                // Clear cache
                                for(int i = 0; i < 63; i++)
                                {
                                    sense = dev.ReadCd(out _, out _, (uint)(wantedLba + 3 + (16 * i)), sectorSize, 16,
                                                       MmcSectorTypes.AllTypes, false, false, false,
                                                       MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                       MmcSubchannel.None, dev.Timeout, out _);

                                    if(sense || dev.Error)
                                        break;
                                }

                                dev.ReadCd(out cmdBuf, out _, wantedLba, sectorSize, 3, MmcSectorTypes.Cdda, false,
                                           false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                           MmcSubchannel.None, dev.Timeout, out _);

                                for(int i = 0; i < cmdBuf.Length - sectorSync.Length; i++)
                                {
                                    Array.Copy(cmdBuf, i, tmpBuf, 0, sectorSync.Length);

                                    if(!tmpBuf.SequenceEqual(sectorSync))
                                        continue;

                                    // De-scramble M and S
                                    minute = cmdBuf[i + 12] ^ 0x01;
                                    second = cmdBuf[i + 13] ^ 0x80;
                                    frame  = cmdBuf[i + 14];

                                    // Convert to binary
                                    minute = (minute / 16 * 10) + (minute & 0x0F);
                                    second = (second / 16 * 10) + (second & 0x0F);
                                    frame  = (frame  / 16 * 10) + (frame  & 0x0F);

                                    // Calculate the first found LBA
                                    lba = (minute * 60 * 75) + (second * 75) + frame - 150;

                                    // Calculate the difference between the found LBA and the requested one
                                    diff = (int)wantedLba - lba;

                                    combinedOffset = i + (2352 * diff);
                                    offsetFound    = true;

                                    break;
                                }
                            }
                        }
                    }
                }

                if(offsetFound)
                    return;

                // Try to get another the offset some other way, we need an audio track just after a data track, same session

                for(int i = 1; i < tracks.Length; i++)
                {
                    if(tracks[i - 1].TrackType == TrackType.Audio ||
                       tracks[i].TrackType     != TrackType.Audio)
                        continue;

                    dataTrack  = tracks[i - 1];
                    audioTrack = tracks[i];

                    break;
                }

                if(dataTrack is null ||
                   audioTrack is null)
                    return;

                // Found them
                sense = dev.ReadCd(out cmdBuf, out _, (uint)audioTrack.TrackStartSector, sectorSize, 3,
                                   MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true, false,
                                   MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                if(sense || dev.Error)
                    return;

                dataTrack.TrackEndSector += 150;

                // Calculate MSF
                minute = (int)dataTrack.TrackEndSector                     / 4500;
                second = ((int)dataTrack.TrackEndSector - (minute * 4500)) / 75;
                frame  = (int)dataTrack.TrackEndSector - (minute * 4500) - (second * 75);

                dataTrack.TrackEndSector -= 150;

                // Convert to BCD
                minute = ((minute / 10) << 4) + (minute % 10);
                second = ((second / 10) << 4) + (second % 10);
                frame  = ((frame  / 10) << 4) + (frame  % 10);

                // Scramble M and S
                minute ^= 0x01;
                second ^= 0x80;

                // Build sync
                sectorSync = new byte[]
                {
                    0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, (byte)minute, (byte)second,
                    (byte)frame
                };

                tmpBuf = new byte[sectorSync.Length];

                for(int i = 0; i < cmdBuf.Length - sectorSync.Length; i++)
                {
                    Array.Copy(cmdBuf, i, tmpBuf, 0, sectorSync.Length);

                    if(!tmpBuf.SequenceEqual(sectorSync))
                        continue;

                    combinedOffset = i + 2352;
                    offsetFound    = true;

                    break;
                }

                if(offsetFound || audioTrack.TrackPregap <= 0)
                    return;

                sense = dev.ReadCd(out byte[] dataBuf, out _, (uint)dataTrack.TrackEndSector, sectorSize, 1,
                                   MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                   MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                if(sense || dev.Error)
                    return;

                for(int i = 0; i < dataBuf.Length; i++)
                    dataBuf[i] ^= Sector.ScrambleTable[i];

                for(int i = 0; i < 2352; i++)
                {
                    byte[] dataSide  = new byte[2352 - i];
                    byte[] audioSide = new byte[2352 - i];

                    Array.Copy(dataBuf, i, dataSide, 0, dataSide.Length);
                    Array.Copy(cmdBuf, 0, audioSide, 0, audioSide.Length);

                    if(!dataSide.SequenceEqual(audioSide))
                        continue;

                    combinedOffset = audioSide.Length;

                    break;
                }
            }
            else
            {
                byte[] videoNowColorFrame = new byte[9 * sectorSize];

                sense = dev.ReadCd(out cmdBuf, out _, 0, sectorSize, 9, MmcSectorTypes.AllTypes, false, false, true,
                                   MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                   dev.Timeout, out _);

                if(sense || dev.Error)
                {
                    sense = dev.ReadCd(out cmdBuf, out _, 0, sectorSize, 9, MmcSectorTypes.Cdda, false, false, true,
                                       MmcHeaderCodes.None, true, true, MmcErrorField.None, MmcSubchannel.None,
                                       dev.Timeout, out _);

                    if(sense || dev.Error)
                    {
                        videoNowColorFrame = null;
                    }
                }

                if(videoNowColorFrame is null)
                {
                    dumpLog?.WriteLine("Could not find VideoNow Color frame offset, dump may not be correct.");
                    updateStatus?.Invoke("Could not find VideoNow Color frame offset, dump may not be correct.");
                }
                else
                {
                    combinedOffset = MMC.GetVideoNowColorOffset(videoNowColorFrame);
                    dumpLog?.WriteLine($"VideoNow Color frame is offset {combinedOffset} bytes.");
                    updateStatus?.Invoke($"VideoNow Color frame is offset {combinedOffset} bytes.");
                }
            }
        }
    }
}