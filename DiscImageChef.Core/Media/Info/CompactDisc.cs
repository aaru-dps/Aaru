using System;
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Core.Logging;
using DiscImageChef.Core.Media.Detection;
using DiscImageChef.Database.Models;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Devices;
using Device = DiscImageChef.Database.Models.Device;

namespace DiscImageChef.Core.Media.Info
{
    public static class CompactDisc
    {
        /// <summary>Gets the offset bytes from a Compact Disc</summary>
        /// <param name="cdOffset">Offset entry from database</param>
        /// <param name="dbDev">Device entry from database</param>
        /// <param name="debug">Debug</param>
        /// <param name="dev">Opened device</param>
        /// <param name="dskType">Detected disk type</param>
        /// <param name="dumpLog">Dump log if applicable</param>
        /// <param name="offsetBytes">Set to combined offset, in bytes</param>
        /// <param name="readcd">If device supports READ CD command</param>
        /// <param name="sectorsForOffset">Sectors needed to fix offset</param>
        /// <param name="tracks">Disc track list</param>
        /// <param name="updateStatus">UpdateStatus event</param>
        /// <returns><c>true</c> if offset could be found, <c>false</c> otherwise</returns>
        public static bool GetOffset(CdOffset cdOffset, Device dbDev, bool debug, DiscImageChef.Devices.Device dev,
                                     MediaType dskType, DumpLog dumpLog, out int offsetBytes, bool readcd,
                                     out int sectorsForOffset, Track[] tracks, UpdateStatusHandler updateStatus)
        {
            byte[]     cmdBuf;
            bool       sense;
            bool       offsetFound = false;
            const uint sectorSize  = 2352;
            offsetBytes      = 0;
            sectorsForOffset = 0;

            if(dskType != MediaType.VideoNowColor)
            {
                if(tracks.All(t => t.TrackType != TrackType.Audio))
                {
                    // No audio tracks so no need to fix offset
                    dumpLog?.WriteLine("No audio tracks, disabling offset fix.");
                    updateStatus?.Invoke("No audio tracks, disabling offset fix.");

                    return false;
                }

                if(!readcd)
                {
                    dumpLog?.WriteLine("READ CD command is not supported, disabling offset fix. Dump may not be correct.");

                    updateStatus?.
                        Invoke("READ CD command is not supported, disabling offset fix. Dump may not be correct.");

                    return false;
                }

                if(tracks.Any(t => t.TrackType != TrackType.Audio))
                {
                    Track dataTrack = tracks.FirstOrDefault(t => t.TrackType != TrackType.Audio);

                    if(dataTrack.TrackSequence != 0)
                    {
                        dataTrack.TrackStartSector += 151;

                        // Calculate MSF
                        ulong minute = dataTrack.TrackStartSector                     / 4500;
                        ulong second = (dataTrack.TrackStartSector - (minute * 4500)) / 75;
                        ulong frame  = dataTrack.TrackStartSector - (minute * 4500) - (second * 75);

                        dataTrack.TrackStartSector -= 151;

                        // Convert to BCD
                        ulong remainder = minute   % 10;
                        minute    = ((minute / 10) * 16) + remainder;
                        remainder = second % 10;
                        second    = ((second / 10) * 16) + remainder;
                        remainder = frame % 10;
                        frame     = ((frame / 10) * 16) + remainder;

                        // Scramble M and S
                        minute ^= 0x01;
                        second ^= 0x80;

                        // Build sync
                        byte[] sectorSync =
                        {
                            0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, (byte)minute,
                            (byte)second, (byte)frame
                        };

                        byte[] tmpBuf = new byte[sectorSync.Length];

                        // Plextor READ CDDA
                        if(dbDev?.ATAPI?.RemovableMedias?.Any(d => d.SupportsPlextorReadCDDA == true) == true ||
                           dbDev?.SCSI?.RemovableMedias?.Any(d => d.SupportsPlextorReadCDDA  == true) == true ||
                           dev.Manufacturer.ToLowerInvariant()                                        == "plextor")
                        {
                            sense = dev.PlextorReadCdDa(out cmdBuf, out _, (uint)dataTrack.TrackStartSector, sectorSize,
                                                        3, PlextorSubchannel.None, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                for(int i = 0; i < cmdBuf.Length - sectorSync.Length; i++)
                                {
                                    Array.Copy(cmdBuf, i, tmpBuf, 0, sectorSync.Length);

                                    if(!tmpBuf.SequenceEqual(sectorSync))
                                        continue;

                                    offsetBytes = i - 2352;
                                    offsetFound = true;

                                    break;
                                }
                            }
                        }

                        if(debug                                                                         ||
                           dbDev?.ATAPI?.RemovableMedias?.Any(d => d.CanReadCdScrambled == true) == true ||
                           dbDev?.SCSI?.RemovableMedias?.Any(d => d.CanReadCdScrambled  == true) == true ||
                           dev.Manufacturer.ToLowerInvariant()                                   == "hl-dt-st")
                        {
                            sense = dev.ReadCd(out cmdBuf, out _, (uint)dataTrack.TrackStartSector, sectorSize, 3,
                                               MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true,
                                               false, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                for(int i = 0; i < cmdBuf.Length - sectorSync.Length; i++)
                                {
                                    Array.Copy(cmdBuf, i, tmpBuf, 0, sectorSync.Length);

                                    if(!tmpBuf.SequenceEqual(sectorSync))
                                        continue;

                                    offsetBytes = i - 2352;
                                    offsetFound = true;

                                    break;
                                }
                            }
                        }
                    }
                }

                // Try to get another the offset some other way, we need an audio track just after a data track, same session
                if(!offsetFound)
                {
                    Track dataTrack  = default;
                    Track audioTrack = default;

                    for(int i = 1; i < tracks.Length; i++)
                    {
                        if(tracks[i - 1].TrackType == TrackType.Audio ||
                           tracks[i].TrackType     != TrackType.Audio)
                            continue;

                        dataTrack  = tracks[i - 1];
                        audioTrack = tracks[i];

                        break;
                    }

                    // Found them
                    if(dataTrack.TrackSequence  != 0 &&
                       audioTrack.TrackSequence != 0)
                    {
                        sense = dev.ReadCd(out cmdBuf, out _, (uint)audioTrack.TrackStartSector, sectorSize, 3,
                                           MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true, false,
                                           MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            dataTrack.TrackEndSector += 150;

                            // Calculate MSF
                            ulong minute = dataTrack.TrackEndSector                     / 4500;
                            ulong second = (dataTrack.TrackEndSector - (minute * 4500)) / 75;
                            ulong frame  = dataTrack.TrackEndSector - (minute * 4500) - (second * 75);

                            dataTrack.TrackEndSector -= 150;

                            // Convert to BCD
                            ulong remainder = minute   % 10;
                            minute    = ((minute / 10) * 16) + remainder;
                            remainder = second % 10;
                            second    = ((second / 10) * 16) + remainder;
                            remainder = frame % 10;
                            frame     = ((frame / 10) * 16) + remainder;

                            // Scramble M and S
                            minute ^= 0x01;
                            second ^= 0x80;

                            // Build sync
                            byte[] sectorSync =
                            {
                                0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, (byte)minute,
                                (byte)second, (byte)frame
                            };

                            byte[] tmpBuf = new byte[sectorSync.Length];

                            for(int i = 0; i < cmdBuf.Length - sectorSync.Length; i++)
                            {
                                Array.Copy(cmdBuf, i, tmpBuf, 0, sectorSync.Length);

                                if(!tmpBuf.SequenceEqual(sectorSync))
                                    continue;

                                offsetBytes = i + 2352;
                                offsetFound = true;

                                break;
                            }

                            if(!offsetFound &&
                               audioTrack.TrackPregap > 0)
                            {
                                sense = dev.ReadCd(out byte[] dataBuf, out _, (uint)dataTrack.TrackEndSector,
                                                   sectorSize, 1, MmcSectorTypes.AllTypes, false, false, true,
                                                   MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                                   MmcSubchannel.None, dev.Timeout, out _);

                                if(!sense &&
                                   !dev.Error)
                                {
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

                                        offsetBytes = audioSide.Length;
                                        offsetFound = true;

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if(cdOffset is null)
                {
                    if(offsetFound)
                    {
                        dumpLog?.WriteLine($"Combined disc and drive offsets are {offsetBytes} bytes");
                        updateStatus?.Invoke($"Combined disc and drive offsets are {offsetBytes} bytes");
                    }
                    else
                    {
                        dumpLog?.
                            WriteLine("Drive read offset is unknown, disabling offset fix. Dump may not be correct.");

                        updateStatus?.
                            Invoke("Drive read offset is unknown, disabling offset fix. Dump may not be correct.");

                        return false;
                    }
                }
                else
                {
                    if(offsetFound)
                    {
                        dumpLog?.
                            WriteLine($"Disc offsets is {offsetBytes - (cdOffset.Offset * 4)} bytes ({(offsetBytes / 4) - cdOffset.Offset} samples)");

                        updateStatus?.
                            Invoke($"Disc offsets is {offsetBytes - (cdOffset.Offset * 4)} bytes ({(offsetBytes / 4) - cdOffset.Offset} samples)");
                    }
                    else
                    {
                        dumpLog?.WriteLine("Disc write offset is unknown, dump may not be correct.");
                        updateStatus?.Invoke("Disc write offset is unknown, dump may not be correct.");

                        offsetBytes = cdOffset.Offset * 4;
                    }

                    dumpLog?.WriteLine($"Offset is {offsetBytes} bytes.");
                    updateStatus?.Invoke($"Offset is {offsetBytes} bytes.");
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
                    offsetBytes = MMC.GetVideoNowColorOffset(videoNowColorFrame);
                    dumpLog?.WriteLine($"VideoNow Color frame is offset {offsetBytes} bytes.");
                    updateStatus?.Invoke($"VideoNow Color frame is offset {offsetBytes} bytes.");
                }
            }

            sectorsForOffset = offsetBytes / (int)sectorSize;

            if(sectorsForOffset < 0)
                sectorsForOffset *= -1;

            if(offsetBytes % sectorSize != 0)
                sectorsForOffset++;

            return offsetFound;
        }
    }
}