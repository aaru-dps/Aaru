// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PlayStationPortable.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Dumping with a jail-broken PlayStation Portable thru USB.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles dumping using a jail-broken PlayStation Portable thru USB.
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
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SCSI;
using Aaru.Devices;

namespace Aaru.Core.Devices.Dumping;

public partial class Dump
{
    static readonly byte[] _fatSignature = "FAT16   "u8.ToArray();
    static readonly byte[] _isoExtension = "ISO"u8.ToArray();

    /// <summary>Dumps a CFW PlayStation Portable UMD</summary>
    void PlayStationPortable()
    {
        if(!_outputPlugin.SupportedMediaTypes.Contains(MediaType.MemoryStickDuo)    &&
           !_outputPlugin.SupportedMediaTypes.Contains(MediaType.MemoryStickProDuo) &&
           !_outputPlugin.SupportedMediaTypes.Contains(MediaType.UMD))
        {
            _dumpLog.WriteLine(Localization.Core.
                                            Selected_output_format_does_not_support_MemoryStick_Duo_or_UMD_cannot_dump);

            StoppingErrorMessage?.Invoke(Localization.Core.
                                                      Selected_output_format_does_not_support_MemoryStick_Duo_or_UMD_cannot_dump);

            return;
        }

        UpdateStatus?.Invoke(Localization.Core.Checking_if_media_is_UMD_or_MemoryStick);
        _dumpLog.WriteLine(Localization.Core.Checking_if_media_is_UMD_or_MemoryStick);

        bool sense = _dev.ModeSense6(out byte[] buffer, out _, false, ScsiModeSensePageControl.Current, 0, _dev.Timeout,
                                     out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Could_not_get_MODE_SENSE);
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_get_MODE_SENSE);

            return;
        }

        Modes.DecodedMode? decoded = Modes.DecodeMode6(buffer, PeripheralDeviceTypes.DirectAccess);

        if(!decoded.HasValue)
        {
            _dumpLog.WriteLine(Localization.Core.Could_not_decode_MODE_SENSE);
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_decode_MODE_SENSE);

            return;
        }

        // UMDs are always write protected
        if(!decoded.Value.Header.WriteProtected)
        {
            DumpMs();

            return;
        }

        sense = _dev.Read12(out buffer, out _, 0, false, true, false, false, 0, 512, 0, 1, false, _dev.Timeout, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Could_not_read);
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_read);

            return;
        }

        var tmp = new byte[8];

        Array.Copy(buffer, 0x36, tmp, 0, 8);

        // UMDs are stored inside a FAT16 volume
        if(!tmp.SequenceEqual(_fatSignature))
        {
            DumpMs();

            return;
        }

        var fatStart      = (ushort)((buffer[0x0F] << 8) + buffer[0x0E]);
        var sectorsPerFat = (ushort)((buffer[0x17] << 8) + buffer[0x16]);
        var rootStart     = (ushort)(sectorsPerFat * 2   + fatStart);

        UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_root_directory_in_sector_0, rootStart));
        _dumpLog.WriteLine(Localization.Core.Reading_root_directory_in_sector_0, rootStart);

        sense = _dev.Read12(out buffer, out _, 0, false, true, false, false, rootStart, 512, 0, 1, false, _dev.Timeout,
                            out _);

        if(sense)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_read);
            _dumpLog.WriteLine(Localization.Core.Could_not_read);

            return;
        }

        tmp = new byte[3];
        Array.Copy(buffer, 0x28, tmp, 0, 3);

        if(!tmp.SequenceEqual(_isoExtension))
        {
            DumpMs();

            return;
        }

        UpdateStatus?.Invoke(string.Format(Localization.Core.FAT_starts_at_sector_0_and_runs_for_1_sectors, fatStart,
                                           sectorsPerFat));

        _dumpLog.WriteLine(Localization.Core.FAT_starts_at_sector_0_and_runs_for_1_sectors, fatStart, sectorsPerFat);

        UpdateStatus?.Invoke(Localization.Core.Reading_FAT);
        _dumpLog.WriteLine(Localization.Core.Reading_FAT);

        var fat = new byte[sectorsPerFat * 512];

        uint position = 0;

        while(position < sectorsPerFat)
        {
            uint transfer = 64;

            if(transfer + position > sectorsPerFat)
                transfer = sectorsPerFat - position;

            sense = _dev.Read12(out buffer, out _, 0, false, true, false, false, position + fatStart, 512, 0, transfer,
                                false, _dev.Timeout, out _);

            if(sense)
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Could_not_read);
                _dumpLog.WriteLine(Localization.Core.Could_not_read);

                return;
            }

            Array.Copy(buffer, 0, fat, position * 512, transfer * 512);

            position += transfer;
        }

        UpdateStatus?.Invoke(Localization.Core.Traversing_FAT);
        _dumpLog.WriteLine(Localization.Core.Traversing_FAT);

        var previousCluster = BitConverter.ToUInt16(fat, 4);

        for(var i = 3; i < fat.Length / 2; i++)
        {
            var nextCluster = BitConverter.ToUInt16(fat, i * 2);

            if(nextCluster == previousCluster + 1)
            {
                previousCluster = nextCluster;

                continue;
            }

            if(nextCluster == 0xFFFF)
                break;

            DumpMs();

            return;
        }

        if(_outputPlugin is IWritableOpticalImage)
            DumpUmd();
        else
            StoppingErrorMessage?.Invoke(Localization.Core.The_specified_image_format_cannot_represent_optical_discs);
    }
}