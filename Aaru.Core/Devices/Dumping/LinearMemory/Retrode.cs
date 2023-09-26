// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Retrode.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Dumping SNES/MD/GEN/MS/N64/GB/GB/GBA carts with a Retrode.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles dumping using a Retrode.
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
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Graphics;
using Humanizer;
using Humanizer.Bytes;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Devices.Dumping;

public partial class Dump
{
    static readonly byte[] _sfcExtension =
    {
        0x53, 0x46, 0x43
    };

    static readonly byte[] _genesisExtension =
    {
        0x42, 0x49, 0x4E
    };

    static readonly byte[] _smsExtension =
    {
        0x53, 0x4D, 0x53
    };

    static readonly byte[] _z64Extension =
    {
        0x5A, 0x36, 0x34
    };

    static readonly byte[] _gbExtension =
    {
        0x47, 0x42, 0x20
    };

    static readonly byte[] _gbcExtension =
    {
        0x47, 0x42, 0x43
    };

    static readonly byte[] _gbaExtension =
    {
        0x47, 0x42, 0x41
    };

    /// <summary>Dumps a game cartridge using a Retrode adapter</summary>
    void Retrode()
    {
        bool sense = _dev.Read10(out byte[] buffer, out _, 0, false, true, false, false, 0, 512, 0, 1, _dev.Timeout,
                                 out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Could_not_read);
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_read);

            return;
        }

        byte[] tmp = new byte[8];

        Array.Copy(buffer, 0x36, tmp, 0, 8);

        // UMDs are stored inside a FAT16 volume
        if(!tmp.SequenceEqual(_fatSignature))
        {
            _dumpLog.WriteLine(Localization.Core.Retrode_partition_not_recognized_not_dumping);
            StoppingErrorMessage?.Invoke(Localization.Core.Retrode_partition_not_recognized_not_dumping);

            return;
        }

        ushort fatStart          = (ushort)((buffer[0x0F] << 8) + buffer[0x0E]);
        ushort sectorsPerFat     = (ushort)((buffer[0x17] << 8) + buffer[0x16]);
        ushort rootStart         = (ushort)((sectorsPerFat * 2) + fatStart);
        ushort rootSize          = (ushort)(((buffer[0x12] << 8) + buffer[0x11]) * 32 / 512);
        byte   sectorsPerCluster = buffer[0x0D];

        UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_root_directory_in_sector_0, rootStart));
        _dumpLog.WriteLine(Localization.Core.Reading_root_directory_in_sector_0, rootStart);

        sense = _dev.Read10(out buffer, out _, 0, false, true, false, false, rootStart, 512, 0, 1, _dev.Timeout, out _);

        if(sense)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_read);
            _dumpLog.WriteLine(Localization.Core.Could_not_read);

            return;
        }

        int  romPos;
        bool sfcFound     = false;
        bool genesisFound = false;
        bool smsFound     = false;
        bool n64Found     = false;
        bool gbFound      = false;
        bool gbcFound     = false;
        bool gbaFound     = false;
        tmp = new byte[3];

        for(romPos = 0; romPos < buffer.Length; romPos += 0x20)
        {
            Array.Copy(buffer, romPos + 8, tmp, 0, 3);

            if(tmp.SequenceEqual(_sfcExtension))
            {
                sfcFound = true;

                break;
            }

            if(tmp.SequenceEqual(_genesisExtension))
            {
                genesisFound = true;

                break;
            }

            if(tmp.SequenceEqual(_smsExtension))
            {
                smsFound = true;

                break;
            }

            if(tmp.SequenceEqual(_z64Extension))
            {
                n64Found = true;

                break;
            }

            if(tmp.SequenceEqual(_gbExtension))
            {
                gbFound = true;

                break;
            }

            if(tmp.SequenceEqual(_gbcExtension))
            {
                gbcFound = true;

                break;
            }

            if(!tmp.SequenceEqual(_gbaExtension))
                continue;

            gbaFound = true;

            break;
        }

        if(!sfcFound     &&
           !genesisFound &&
           !smsFound     &&
           !n64Found)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.No_cartridge_found_not_dumping);
            _dumpLog.WriteLine(Localization.Core.No_cartridge_found_not_dumping);

            return;
        }

        ushort cluster = BitConverter.ToUInt16(buffer, romPos + 0x1A);
        uint   romSize = BitConverter.ToUInt32(buffer, romPos + 0x1C);

        MediaType mediaType = gbaFound
                                  ? MediaType.GameBoyAdvanceGamePak
                                  : gbFound || gbcFound
                                      ? MediaType.GameBoyGamePak
                                      : n64Found
                                          ? MediaType.N64GamePak
                                          : smsFound
                                              ? MediaType.MasterSystemCartridge
                                              : genesisFound
                                                  ? MediaType.MegaDriveCartridge
                                                  : MediaType.SNESGamePak;

        uint   blocksToRead  = 64;
        double totalDuration = 0;
        double currentSpeed  = 0;
        double maxSpeed      = double.MinValue;
        double minSpeed      = double.MaxValue;
        byte[] senseBuf;

        if(_outputPlugin is not IByteAddressableImage outputBai ||
           !outputBai.SupportedMediaTypes.Contains(mediaType))
        {
            _dumpLog.WriteLine(Localization.Core.The_specified_format_does_not_support_the_inserted_cartridge);

            StoppingErrorMessage?.Invoke(Localization.Core.
                                                      The_specified_format_does_not_support_the_inserted_cartridge);

            return;
        }

        sense = _dev.Read10(out byte[] readBuffer, out _, 0, false, true, false, false, 0, 512, 0, 1, _dev.Timeout,
                            out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Could_not_read);
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_read);

            return;
        }

        uint startSector  = (uint)(rootStart + rootSize + ((cluster - 2) * sectorsPerCluster));
        uint romSectors   = romSize / 512;
        uint romRemaining = romSize % 512;

        switch(romSize)
        {
            case > 1073741824:
                UpdateStatus?.Invoke(string.Format(Localization.Core.Cartridge_has_0_bytes_1_GiB, romSize,
                                                   romSize / 1073741824d));

                break;
            case > 1048576:
                UpdateStatus?.Invoke(string.Format(Localization.Core.Cartridge_has_0_bytes_1_MiB, romSize,
                                                   romSize / 1048576d));

                break;
            case > 1024:
                UpdateStatus?.Invoke(string.Format(Localization.Core.Cartridge_has_0_bytes_1_KiB, romSize,
                                                   romSize / 1024d));

                break;
            default:
                UpdateStatus?.Invoke(string.Format(Localization.Core.Cartridge_has_0_bytes, romSize));

                break;
        }

        UpdateStatus?.Invoke(string.Format(Localization.Core.Media_identified_as_0, mediaType));
        _dumpLog.WriteLine(Localization.Core.Media_identified_as_0, mediaType);

        ErrorNumber ret = outputBai.Create(_outputPath, mediaType, _formatOptions, romSize);

        // Cannot create image
        if(ret != ErrorNumber.NoError)
        {
            _dumpLog.WriteLine(Localization.Core.Error_0_creating_output_image_not_continuing, ret);
            _dumpLog.WriteLine(outputBai.ErrorMessage);

            StoppingErrorMessage?.Invoke(Localization.Core.Error_creating_output_image_not_continuing +
                                         Environment.NewLine + outputBai.ErrorMessage);

            return;
        }

        if(_createGraph)
            _mediaGraph = new BlockMap((int)_dimensions, (int)_dimensions, romSectors);

        DateTime start              = DateTime.UtcNow;
        double   imageWriteDuration = 0;

        DateTime timeSpeedStart   = DateTime.UtcNow;
        ulong    sectorSpeedStart = 0;
        InitProgress?.Invoke();

        for(ulong i = 0; i < romSectors; i += blocksToRead)
        {
            if(_aborted)
            {
                UpdateStatus?.Invoke(Localization.Core.Aborted);
                _dumpLog.WriteLine(Localization.Core.Aborted);

                break;
            }

            if(romSectors - i < blocksToRead)
                blocksToRead = (uint)(romSectors - i);

            if(currentSpeed > maxSpeed &&
               currentSpeed > 0)
                maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed &&
               currentSpeed > 0)
                minSpeed = currentSpeed;

            UpdateProgress?.
                Invoke(string.Format(Localization.Core.Reading_byte_0_of_1_2, i * 512, romSize, ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond)),
                       (long)i * 512, romSize);

            sense = _dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)(startSector + i),
                                512, 0, (ushort)blocksToRead, _dev.Timeout, out double cmdDuration);

            totalDuration += cmdDuration;

            if(!sense &&
               !_dev.Error)
            {
                DateTime writeStart = DateTime.Now;
                outputBai.WriteBytes(readBuffer, 0, readBuffer.Length, out _);
                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                _mediaGraph.PaintSectorsGood(i, blocksToRead);
            }
            else
            {
                _errorLog?.WriteLine(i, _dev.Error, _dev.LastError, senseBuf);

                // TODO: Reset device after X errors
                if(_stopOnError)
                    return; // TODO: Return more cleanly

                _dumpLog.WriteLine(Localization.Core.Skipping_0_bytes_from_errored_byte_1, _skip * 512, i * 512);
                i += _skip - blocksToRead;
            }

            sectorSpeedStart += blocksToRead;

            double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

            if(elapsed <= 0)
                continue;

            currentSpeed     = sectorSpeedStart * 512 / (1048576 * elapsed);
            sectorSpeedStart = 0;
            timeSpeedStart   = DateTime.UtcNow;
        }

        if(romRemaining > 0 &&
           !_aborted)
        {
            if(currentSpeed > maxSpeed &&
               currentSpeed > 0)
                maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed &&
               currentSpeed > 0)
                minSpeed = currentSpeed;

            UpdateProgress?.
                Invoke(string.Format(Localization.Core.Reading_byte_0_of_1_2, romSectors * 512, romSize, ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond)),
                       (long)romSectors * 512, romSize);

            sense = _dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, romSectors, 512, 0, 1,
                                _dev.Timeout, out double cmdDuration);

            totalDuration += cmdDuration;

            if(!sense &&
               !_dev.Error)
            {
                DateTime writeStart = DateTime.Now;
                outputBai.WriteBytes(readBuffer, 0, (int)romRemaining, out _);
                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
            }
            else
            {
                _errorLog?.WriteLine(romSectors, _dev.Error, _dev.LastError, senseBuf);

                // TODO: Reset device after X errors
                if(_stopOnError)
                    return; // TODO: Return more cleanly

                _dumpLog.WriteLine(Localization.Core.Skipping_0_bytes_from_errored_byte_1, _skip * 512,
                                   romSectors                                                    * 512);
            }
        }

        DateTime end = DateTime.UtcNow;
        EndProgress?.Invoke();

        UpdateStatus?.Invoke(string.Format(Localization.Core.Dump_finished_in_0_seconds, (end - start).TotalSeconds));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_dump_speed_0,
                                           ByteSize.FromBytes(512 * (romSectors + 1)).
                                                    Per(totalDuration.Milliseconds())));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_write_speed_0,
                                           ByteSize.FromBytes(512 * (romSectors + 1)).
                                                    Per(imageWriteDuration.Seconds())));

        _dumpLog.WriteLine(Localization.Core.Dump_finished_in_0_seconds, (end - start).TotalSeconds);

        _dumpLog.WriteLine(string.Format(Localization.Core.Average_dump_speed_0,
                                         ByteSize.FromBytes(512 * (romSectors + 1)).Per(totalDuration.Milliseconds())));

        _dumpLog.WriteLine(string.Format(Localization.Core.Average_write_speed_0,
                                         ByteSize.FromBytes(512 * (romSectors + 1)).Per(imageWriteDuration.Seconds())));

        var metadata = new CommonTypes.Structs.ImageInfo
        {
            Application        = "Aaru",
            ApplicationVersion = Version.GetVersion()
        };

        if(!outputBai.SetImageInfo(metadata))
            ErrorMessage?.Invoke(Localization.Core.Error_0_setting_metadata + Environment.NewLine +
                                 outputBai.ErrorMessage);

        // TODO: Set dump hardware
        //outputBAI.SetDumpHardware();

        if(_preSidecar != null)
            outputBai.SetMetadata(_preSidecar);

        _dumpLog.WriteLine(Localization.Core.Closing_output_file);
        UpdateStatus?.Invoke(Localization.Core.Closing_output_file);
        DateTime closeStart = DateTime.Now;
        outputBai.Close();
        DateTime closeEnd = DateTime.Now;
        _dumpLog.WriteLine(Localization.Core.Closed_in_0_seconds, (closeEnd - closeStart).TotalSeconds);

        if(_aborted)
        {
            UpdateStatus?.Invoke(Localization.Core.Aborted);
            _dumpLog.WriteLine(Localization.Core.Aborted);

            return;
        }

        double totalChkDuration = 0;

        /* TODO: Create sidecar
        if(_metadata)
            WriteOpticalSidecar(blockSize, blocks, mediaType, null, null, 1, out totalChkDuration, null);
        */
        UpdateStatus?.Invoke("");

        UpdateStatus?.
            Invoke(string.Format(Localization.Core.Took_a_total_of_0_seconds_1_processing_commands_2_checksumming_3_writing_4_closing,
                                 (end - start).TotalSeconds, totalDuration / 1000, totalChkDuration / 1000,
                                 imageWriteDuration, (closeEnd - closeStart).TotalSeconds));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_speed_0,
                                           ByteSize.FromBytes(512 * (romSectors + 1)).
                                                    Per(totalDuration.Milliseconds())));

        if(maxSpeed > 0)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Fastest_speed_burst_0, ByteSize.FromMegabytes(maxSpeed).Per(_oneSecond)));

        if(minSpeed is > 0 and < double.MaxValue)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Slowest_speed_burst_0,
                                               ByteSize.FromMegabytes(minSpeed).Per(_oneSecond)));

        UpdateStatus?.Invoke("");

        Statistics.AddMedia(mediaType, true);
    }
}