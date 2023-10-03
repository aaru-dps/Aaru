// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ReaderSCSI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains common code for reading SCSI devices.
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
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Decoders.SCSI;

namespace Aaru.Core.Devices;

sealed partial class Reader
{
    // TODO: Raw reading
    bool _hldtstReadRaw;
    bool _plextorReadRaw;
    bool _read10;
    bool _read12;
    bool _read16;
    bool _read6;
    bool _readLong10;
    bool _readLong16;
    bool _readLongDvd;
    bool _seek10;
    bool _seek6;
    bool _syqReadLong10;
    bool _syqReadLong6;

    ulong ScsiGetBlocks() => ScsiGetBlockSize() ? 0 : Blocks;

    bool ScsiFindReadCommand()
    {
        if(Blocks == 0)
            GetDeviceBlocks();

        if(Blocks == 0)
            return true;

        byte[] senseBuf;
        int    tries      = 0;
        uint   lba        = 0;
        bool   mediumScan = false;

        if(_dev.ScsiType == PeripheralDeviceTypes.OpticalDevice)
        {
            mediumScan = !_dev.MediumScan(out _, true, false, false, false, false, lba, 1, (uint)Blocks,
                                          out uint foundLba, out _, _timeout, out _);

            if(mediumScan)
                lba = foundLba;
        }

        var rnd = new Random();

        while(tries < 10)
        {
            _read6 = !_dev.Read6(out _, out senseBuf, lba, LogicalBlockSize, _timeout, out _);

            _read10 = !_dev.Read10(out _, out senseBuf, 0, false, false, false, false, lba, LogicalBlockSize, 0, 1,
                                   _timeout, out _);

            _read12 = !_dev.Read12(out _, out senseBuf, 0, false, false, false, false, lba, LogicalBlockSize, 0, 1,
                                   false, _timeout, out _);

            _read16 = !_dev.Read16(out _, out senseBuf, 0, false, false, false, lba, LogicalBlockSize, 0, 1, false,
                                   _timeout, out _);

            if(_read6  ||
               _read10 ||
               _read12 ||
               _read16)
                break;

            lba = (uint)rnd.Next(1, (int)Blocks);

            if(mediumScan)
            {
                mediumScan = !_dev.MediumScan(out _, true, false, false, false, false, lba, 1, (uint)Blocks,
                                              out uint foundLba, out _, _timeout, out _);

                if(mediumScan)
                    lba = foundLba;
            }

            tries++;
        }

        switch(_read6)
        {
            case false when !_read10 && !_read12 && !_read16:
            {
                // Magneto-opticals may have empty LBA 0 but we know they work with READ(12)
                if(_dev.ScsiType == PeripheralDeviceTypes.OpticalDevice)
                {
                    ErrorMessage = Localization.Core.Cannot_read_medium_aborting_scan;

                    return true;
                }

                _read12 = true;

                break;
            }
            case true when !_read10 && !_read12 && !_read16 && Blocks > 0x001FFFFF + 1:
                ErrorMessage =
                    string.Format(Localization.Core.Device_only_supports_SCSI_READ_6_but_has_more_than_0_blocks_1_blocks_total,
                                  0x001FFFFF + 1, Blocks);

                return true;
        }

        if(Blocks > 0x001FFFFF + 1)
            _read6 = false;

        if(_read10)
            _read12 = false;

        if(!_read16 &&
           Blocks > 0xFFFFFFFF + (long)1)
        {
            ErrorMessage =
                string.Format(Localization.Core.Device_only_supports_SCSI_READ_10_but_has_more_than_0_blocks_1_blocks_total,
                              0xFFFFFFFF + (long)1, Blocks);

            return true;
        }

        if(Blocks > 0xFFFFFFFF + (long)1)
        {
            _read10 = false;
            _read12 = false;
        }

        _seek6 = !_dev.Seek6(out senseBuf, lba, _timeout, out _);

        _seek10 = !_dev.Seek10(out senseBuf, lba, _timeout, out _);

        if(CanReadRaw)
        {
            bool testSense;
            CanReadRaw = false;

            if(_dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
            {
                /*testSense = dev.ReadLong16(out readBuffer, out senseBuf, false, 0, 0xFFFF, timeout, out duration);
                if (testSense && !dev.Error)
                {
                    decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                    if (decSense.HasValue)
                    {
                        if (decSense.Value.SenseKey == Aaru.Decoders.SCSI.SenseKeys.IllegalRequest &&
                            decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                        {
                            readRaw = true;
                            if (decSense.Value.InformationValid && decSense.Value.ILI)
                            {
                                longBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                                readLong16 = !dev.ReadLong16(out readBuffer, out senseBuf, false, 0, longBlockSize, timeout, out duration);
                            }
                        }
                    }
                }*/

                testSense = _dev.ReadLong10(out _, out senseBuf, false, false, 0, 0xFFFF, _timeout, out _);
                DecodedSense? decSense;

                if(testSense && !_dev.Error)
                {
                    decSense = Sense.Decode(senseBuf);

                    if(decSense is { SenseKey: SenseKeys.IllegalRequest, ASC: 0x24, ASCQ: 0x00 })
                    {
                        CanReadRaw = true;

                        bool valid       = decSense?.Fixed?.InformationValid == true;
                        bool ili         = decSense?.Fixed?.ILI              == true;
                        uint information = decSense?.Fixed?.Information ?? 0;

                        if(decSense.Value.Descriptor.HasValue &&
                           decSense.Value.Descriptor.Value.Descriptors.TryGetValue(0, out byte[] desc00))
                        {
                            valid       = true;
                            ili         = true;
                            information = (uint)Sense.DecodeDescriptor00(desc00);

                            if(decSense.Value.Descriptor.Value.Descriptors.TryGetValue(4, out byte[] desc04))
                                Sense.DecodeDescriptor04(desc04, out _, out _, out ili);
                        }

                        if(valid && ili)
                        {
                            LongBlockSize = 0xFFFF - (information & 0xFFFF);

                            _readLong10 = !_dev.ReadLong10(out _, out senseBuf, false, false, 0, (ushort)LongBlockSize,
                                                           _timeout, out _);
                        }
                    }
                }

                if(CanReadRaw && LongBlockSize == LogicalBlockSize)
                    switch(LogicalBlockSize)
                    {
                        case 512:
                        {
                            foreach(ushort testSize in new ushort[]
                                    {
                                        // Long sector sizes for floppies
                                        514,

                                        // Long sector sizes for SuperDisk
                                        536, 558,

                                        // Long sector sizes for 512-byte magneto-opticals
                                        600, 610, 630
                                    })
                            {
                                testSense = _dev.ReadLong16(out _, out senseBuf, false, 0, testSize, _timeout, out _);

                                if(!testSense &&
                                   !_dev.Error)
                                {
                                    _readLong16   = true;
                                    LongBlockSize = testSize;
                                    CanReadRaw    = true;

                                    break;
                                }

                                testSense = _dev.ReadLong10(out _, out senseBuf, false, false, 0, testSize, _timeout,
                                                            out _);

                                if(testSense || _dev.Error)
                                    continue;

                                _readLong10   = true;
                                LongBlockSize = testSize;
                                CanReadRaw    = true;

                                break;
                            }

                            break;
                        }
                        case 1024:
                        {
                            foreach(ushort testSize in new ushort[]
                                    {
                                        // Long sector sizes for floppies
                                        1026,

                                        // Long sector sizes for 1024-byte magneto-opticals
                                        1200
                                    })
                            {
                                testSense = _dev.ReadLong16(out _, out senseBuf, false, 0, testSize, _timeout, out _);

                                if(!testSense &&
                                   !_dev.Error)
                                {
                                    _readLong16   = true;
                                    LongBlockSize = testSize;
                                    CanReadRaw    = true;

                                    break;
                                }

                                testSense = _dev.ReadLong10(out _, out senseBuf, false, false, 0, testSize, _timeout,
                                                            out _);

                                if(testSense || _dev.Error)
                                    continue;

                                _readLong10   = true;
                                LongBlockSize = testSize;
                                CanReadRaw    = true;

                                break;
                            }

                            break;
                        }
                        case 2048:
                        {
                            testSense = _dev.ReadLong16(out _, out senseBuf, false, 0, 2380, _timeout, out _);

                            if(!testSense &&
                               !_dev.Error)
                            {
                                _readLong16   = true;
                                LongBlockSize = 2380;
                                CanReadRaw    = true;
                            }
                            else
                            {
                                testSense = _dev.ReadLong10(out _, out senseBuf, false, false, 0, 2380, _timeout,
                                                            out _);

                                if(!testSense &&
                                   !_dev.Error)
                                {
                                    _readLong10   = true;
                                    LongBlockSize = 2380;
                                    CanReadRaw    = true;
                                }
                            }

                            break;
                        }
                        case 4096:
                        {
                            testSense = _dev.ReadLong16(out _, out senseBuf, false, 0, 4760, _timeout, out _);

                            if(!testSense &&
                               !_dev.Error)
                            {
                                _readLong16   = true;
                                LongBlockSize = 4760;
                                CanReadRaw    = true;
                            }
                            else
                            {
                                testSense = _dev.ReadLong10(out _, out senseBuf, false, false, 0, 4760, _timeout,
                                                            out _);

                                if(!testSense &&
                                   !_dev.Error)
                                {
                                    _readLong10   = true;
                                    LongBlockSize = 4760;
                                    CanReadRaw    = true;
                                }
                            }

                            break;
                        }
                        case 8192:
                        {
                            testSense = _dev.ReadLong16(out _, out senseBuf, false, 0, 9424, _timeout, out _);

                            if(!testSense &&
                               !_dev.Error)
                            {
                                _readLong16   = true;
                                LongBlockSize = 9424;
                                CanReadRaw    = true;
                            }
                            else
                            {
                                testSense = _dev.ReadLong10(out _, out senseBuf, false, false, 0, 9424, _timeout,
                                                            out _);

                                if(!testSense &&
                                   !_dev.Error)
                                {
                                    _readLong10   = true;
                                    LongBlockSize = 9424;
                                    CanReadRaw    = true;
                                }
                            }

                            break;
                        }
                    }

                if(!CanReadRaw &&
                   _dev.Manufacturer == "SYQUEST")
                {
                    testSense = _dev.SyQuestReadLong10(out _, out senseBuf, 0, 0xFFFF, _timeout, out _);

                    if(testSense)
                    {
                        decSense = Sense.Decode(senseBuf);

                        if(decSense.HasValue)
                            if(decSense.Value.SenseKey == SenseKeys.IllegalRequest &&
                               decSense.Value.ASC      == 0x24                     &&
                               decSense.Value.ASCQ     == 0x00)
                            {
                                CanReadRaw = true;

                                bool valid       = decSense?.Fixed?.InformationValid == true;
                                bool ili         = decSense?.Fixed?.ILI              == true;
                                uint information = decSense?.Fixed?.Information ?? 0;

                                if(decSense.Value.Descriptor.HasValue &&
                                   decSense.Value.Descriptor.Value.Descriptors.TryGetValue(0, out byte[] desc00))
                                {
                                    valid       = true;
                                    ili         = true;
                                    information = (uint)Sense.DecodeDescriptor00(desc00);

                                    if(decSense.Value.Descriptor.Value.Descriptors.TryGetValue(4, out byte[] desc04))
                                        Sense.DecodeDescriptor04(desc04, out _, out _, out ili);
                                }

                                if(valid && ili)
                                {
                                    LongBlockSize = 0xFFFF - (information & 0xFFFF);

                                    _syqReadLong10 =
                                        !_dev.SyQuestReadLong10(out _, out senseBuf, 0, LongBlockSize, _timeout, out _);
                                }
                            }
                            else
                            {
                                testSense = _dev.SyQuestReadLong6(out _, out senseBuf, 0, 0xFFFF, _timeout, out _);

                                if(testSense)
                                {
                                    decSense = Sense.Decode(senseBuf);

                                    if(decSense is { SenseKey: SenseKeys.IllegalRequest, ASC: 0x24, ASCQ: 0x00 })
                                    {
                                        CanReadRaw = true;

                                        bool valid       = decSense?.Fixed?.InformationValid == true;
                                        bool ili         = decSense?.Fixed?.ILI              == true;
                                        uint information = decSense?.Fixed?.Information ?? 0;

                                        if(decSense.Value.Descriptor.HasValue &&
                                           decSense.Value.Descriptor.Value.Descriptors.
                                                    TryGetValue(0, out byte[] desc00))
                                        {
                                            valid       = true;
                                            ili         = true;
                                            information = (uint)Sense.DecodeDescriptor00(desc00);

                                            if(decSense.Value.Descriptor.Value.Descriptors.
                                                        TryGetValue(4, out byte[] desc04))
                                                Sense.DecodeDescriptor04(desc04, out _, out _, out ili);
                                        }

                                        if(valid && ili)
                                        {
                                            LongBlockSize = 0xFFFF - (information & 0xFFFF);

                                            _syqReadLong6 =
                                                !_dev.SyQuestReadLong6(out _, out senseBuf, 0, LongBlockSize, _timeout,
                                                                       out _);
                                        }
                                    }
                                }
                            }
                    }

                    if(!CanReadRaw &&
                       LogicalBlockSize == 256)
                    {
                        testSense = _dev.SyQuestReadLong6(out _, out senseBuf, 0, 262, _timeout, out _);

                        if(!testSense &&
                           !_dev.Error)
                        {
                            _syqReadLong6 = true;
                            LongBlockSize = 262;
                            CanReadRaw    = true;
                        }
                    }
                }
            }
            else
            {
                switch(_dev.Manufacturer)
                {
                    case "HL-DT-ST":
                        _hldtstReadRaw = !_dev.HlDtStReadRawDvd(out _, out senseBuf, 0, 1, _timeout, out _);

                        break;
                    case "PLEXTOR":
                        _plextorReadRaw = !_dev.PlextorReadRawDvd(out _, out senseBuf, 0, 1, _timeout, out _);

                        break;
                }

                if(_hldtstReadRaw || _plextorReadRaw)
                {
                    CanReadRaw    = true;
                    LongBlockSize = 2064;
                }

                // READ LONG (10) for some DVD drives
                if(!CanReadRaw &&
                   _dev.Manufacturer == "MATSHITA")
                {
                    testSense = _dev.ReadLong10(out _, out senseBuf, false, false, 0, 37856, _timeout, out _);

                    if(!testSense &&
                       !_dev.Error)
                    {
                        _readLongDvd  = true;
                        LongBlockSize = 37856;
                        CanReadRaw    = true;
                    }
                }
            }
        }

        if(CanReadRaw)
        {
            if(_readLong16)
                AaruConsole.WriteLine(Localization.Core.Using_SCSI_READ_LONG_16_command);
            else if(_readLong10 || _readLongDvd)
                AaruConsole.WriteLine(Localization.Core.Using_SCSI_READ_LONG_10_command);
            else if(_syqReadLong10)
                AaruConsole.WriteLine(Localization.Core.Using_SyQuest_READ_LONG_10_command);
            else if(_syqReadLong6)
                AaruConsole.WriteLine(Localization.Core.Using_SyQuest_READ_LONG_6_command);
            else if(_hldtstReadRaw)
                AaruConsole.WriteLine(Localization.Core.Using_HL_DT_ST_raw_DVD_reading);
            else if(_plextorReadRaw)
                AaruConsole.WriteLine(Localization.Core.Using_Plextor_raw_DVD_reading);
        }
        else if(_read6)
            AaruConsole.WriteLine(Localization.Core.Using_SCSI_READ_6_command);
        else if(_read10)
            AaruConsole.WriteLine(Localization.Core.Using_SCSI_READ_10_command);
        else if(_read12)
            AaruConsole.WriteLine(Localization.Core.Using_SCSI_READ_12_command);
        else if(_read16)
            AaruConsole.WriteLine(Localization.Core.Using_SCSI_READ_16_command);

        return false;
    }

    bool ScsiGetBlockSize()
    {
        Blocks = 0;

        bool sense = _dev.ReadCapacity(out byte[] cmdBuf, out byte[] senseBuf, _timeout, out _);

        if(!sense)
        {
            Blocks = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]) & 0xFFFFFFFF;
            LogicalBlockSize = (uint)((cmdBuf[4] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + cmdBuf[7]);
        }

        if(sense || Blocks == 0xFFFFFFFF)
        {
            sense = _dev.ReadCapacity16(out cmdBuf, out senseBuf, _timeout, out _);

            switch(sense)
            {
                case true when Blocks == 0 && _dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice:
                    ErrorMessage = string.Format(Localization.Core.Unable_to_get_media_capacity,
                                                 Sense.PrettifySense(senseBuf));

                    return true;
                case false:
                {
                    byte[] temp = new byte[8];

                    Array.Copy(cmdBuf, 0, temp, 0, 8);
                    Array.Reverse(temp);
                    Blocks           = BitConverter.ToUInt64(temp, 0);
                    LogicalBlockSize = (uint)((cmdBuf[8] << 24) + (cmdBuf[9] << 16) + (cmdBuf[10] << 8) + cmdBuf[11]);

                    break;
                }
            }
        }

        PhysicalBlockSize = LogicalBlockSize;
        LongBlockSize     = LogicalBlockSize;

        return false;
    }

    bool ScsiGetBlocksToRead(uint startWithBlocks)
    {
        BlocksToRead = startWithBlocks;

        while(true)
        {
            if(_read6)
            {
                _dev.Read6(out _, out _, 0, LogicalBlockSize, (byte)BlocksToRead, _timeout, out _);

                if(_dev.Error)
                    BlocksToRead /= 2;
            }
            else if(_read10)
            {
                _dev.Read10(out _, out _, 0, false, true, false, false, 0, LogicalBlockSize, 0, (ushort)BlocksToRead,
                            _timeout, out _);

                if(_dev.Error)
                    BlocksToRead /= 2;
            }
            else if(_read12)
            {
                _dev.Read12(out _, out _, 0, false, false, false, false, 0, LogicalBlockSize, 0, BlocksToRead, false,
                            _timeout, out _);

                if(_dev.Error)
                    BlocksToRead /= 2;
            }
            else if(_read16)
            {
                _dev.Read16(out _, out _, 0, false, true, false, 0, LogicalBlockSize, 0, BlocksToRead, false, _timeout,
                            out _);

                if(_dev.Error)
                    BlocksToRead /= 2;
            }

            if(!_dev.Error ||
               BlocksToRead == 1)
                break;
        }

        if(!_dev.Error)
            return false;

        // Magneto-opticals may have LBA 0 empty, we can hard code the value to a safe one
        if(_dev.ScsiType == PeripheralDeviceTypes.OpticalDevice)
        {
            BlocksToRead = 16;

            return false;
        }

        BlocksToRead = 1;

        ErrorMessage = string.Format(Localization.Core.Device_error_0_trying_to_guess_ideal_transfer_length,
                                     _dev.LastError);

        return true;
    }

    bool ScsiReadBlocks(out byte[] buffer, ulong block, uint count, out double duration, out bool recoveredError,
                        out bool blankCheck)
    {
        bool   sense;
        byte[] senseBuf;
        buffer         = null;
        duration       = 0;
        recoveredError = false;
        blankCheck     = false;

        if(CanReadRaw)
            if(_readLong16)
                sense = _dev.ReadLong16(out buffer, out senseBuf, false, block, LongBlockSize, _timeout, out duration);
            else if(_readLong10)
                sense = _dev.ReadLong10(out buffer, out senseBuf, false, false, (uint)block, (ushort)LongBlockSize,
                                        _timeout, out duration);
            else if(_syqReadLong10)
                sense = _dev.SyQuestReadLong10(out buffer, out senseBuf, (uint)block, LongBlockSize, _timeout,
                                               out duration);
            else if(_syqReadLong6)
                sense = _dev.SyQuestReadLong6(out buffer, out senseBuf, (uint)block, LongBlockSize, _timeout,
                                              out duration);
            else if(_hldtstReadRaw)
                sense = _dev.HlDtStReadRawDvd(out buffer, out senseBuf, (uint)block, LongBlockSize, _timeout,
                                              out duration);
            else if(_plextorReadRaw)
                sense = _dev.PlextorReadRawDvd(out buffer, out senseBuf, (uint)block, LongBlockSize, _timeout,
                                               out duration);
            else
                return true;
        else
        {
            if(_read6)
                sense = _dev.Read6(out buffer, out senseBuf, (uint)block, LogicalBlockSize, (byte)count, _timeout,
                                   out duration);
            else if(_read10)
                sense = _dev.Read10(out buffer, out senseBuf, 0, false, false, false, false, (uint)block,
                                    LogicalBlockSize, 0, (ushort)count, _timeout, out duration);
            else if(_read12)
                sense = _dev.Read12(out buffer, out senseBuf, 0, false, false, false, false, (uint)block,
                                    LogicalBlockSize, 0, count, false, _timeout, out duration);
            else if(_read16)
                sense = _dev.Read16(out buffer, out senseBuf, 0, false, false, false, block, LogicalBlockSize, 0, count,
                                    false, _timeout, out duration);
            else
                return true;
        }

        if(sense || _dev.Error)
            _errorLog?.WriteLine(block, _dev.Error, _dev.LastError, senseBuf);

        if(!sense &&
           !_dev.Error)
            return false;

        recoveredError = Sense.Decode(senseBuf)?.SenseKey == SenseKeys.RecoveredError;

        blankCheck = Sense.Decode(senseBuf)?.SenseKey == SenseKeys.BlankCheck;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.Core.READ_error_0, Sense.PrettifySense(senseBuf));

        return sense;
    }

    bool ScsiSeek(ulong block, out double duration)
    {
        bool sense = true;
        duration = 0;

        if(_seek6)
            sense = _dev.Seek6(out _, (uint)block, _timeout, out duration);
        else if(_seek10)
            sense = _dev.Seek10(out _, (uint)block, _timeout, out duration);

        return sense;
    }
}