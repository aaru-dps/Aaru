// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Reader.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains common code for reading devices.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.Core.Logging;
using Aaru.Devices;

namespace Aaru.Core.Devices;

/// <summary>Reduces common code used for scanning and dumping</summary>
sealed partial class Reader
{
    const    string   ATA_MODULE_NAME  = "ATA Reader";
    const    string   SCSI_MODULE_NAME = "SCSI Reader";
    readonly Device   _dev;
    readonly ErrorLog _errorLog;
    readonly uint     _timeout;

    internal Reader(Device dev, uint timeout, byte[] identification, ErrorLog errorLog, bool raw = false)
    {
        _dev         = dev;
        _timeout     = timeout;
        BlocksToRead = 64;
        CanReadRaw   = raw;
        _errorLog    = errorLog;

        switch(dev.Type)
        {
            case DeviceType.ATA:
                Identify.IdentifyDevice? ataIdNullable = Identify.Decode(identification);

                if(ataIdNullable.HasValue)
                    _ataId = ataIdNullable.Value;

                break;
            case DeviceType.NVMe:
                throw new NotImplementedException(Localization.Core.NVMe_devices_not_yet_supported);
        }
    }

    internal string ErrorMessage      { get; private set; }
    internal ulong  Blocks            { get; private set; }
    internal uint   BlocksToRead      { get; private set; }
    internal uint   LogicalBlockSize  { get; private set; }
    internal uint   PhysicalBlockSize { get; private set; }
    internal uint   LongBlockSize     { get; private set; }
    internal bool   CanReadRaw        { get; private set; }
    internal bool   CanSeek           => _ataSeek    || _seek6 || _seek10;
    internal bool   CanSeekLba        => _ataSeekLba || _seek6 || _seek10;

    internal ulong GetDeviceBlocks()
    {
        switch(_dev.Type)
        {
            case DeviceType.ATA:
                return AtaGetBlocks();
            case DeviceType.ATAPI:
            case DeviceType.SCSI:
                return ScsiGetBlocks();
            default:
                ErrorMessage = string.Format(Localization.Core.Unknown_device_type_0, _dev.Type);

                return 0;
        }
    }

    internal bool FindReadCommand()
    {
        switch(_dev.Type)
        {
            case DeviceType.ATA:
                return AtaFindReadCommand();
            case DeviceType.ATAPI:
            case DeviceType.SCSI:
                return ScsiFindReadCommand();
            default:
                ErrorMessage = string.Format(Localization.Core.Unknown_device_type_0, _dev.Type);

                return true;
        }
    }

    internal bool GetBlockSize()
    {
        switch(_dev.Type)
        {
            case DeviceType.ATA:
                return AtaGetBlockSize();
            case DeviceType.ATAPI:
            case DeviceType.SCSI:
                return ScsiGetBlockSize();
            default:
                ErrorMessage = string.Format(Localization.Core.Unknown_device_type_0, _dev.Type);

                return true;
        }
    }

    internal bool GetBlocksToRead(uint startWithBlocks = 64)
    {
        switch(_dev.Type)
        {
            case DeviceType.ATA:
                return AtaGetBlocksToRead(startWithBlocks);
            case DeviceType.ATAPI:
            case DeviceType.SCSI:
                return ScsiGetBlocksToRead(startWithBlocks);
            default:
                ErrorMessage = string.Format(Localization.Core.Unknown_device_type_0, _dev.Type);

                return true;
        }
    }

    internal bool ReadBlock(out byte[] buffer, ulong block, out double duration, out bool recoveredError,
                            out bool   blankCheck) =>
        ReadBlocks(out buffer, block, 1, out duration, out recoveredError, out blankCheck);

    internal bool ReadBlocks(out byte[] buffer, ulong block, out double duration, out bool recoveredError,
                             out bool   blankCheck) => ReadBlocks(out buffer,         block, BlocksToRead, out duration,
                                                                  out recoveredError, out blankCheck);

    internal bool ReadBlocks(out byte[] buffer, ulong block, uint count, out double duration, out bool recoveredError,
                             out bool   blankCheck)
    {
        switch(_dev.Type)
        {
            case DeviceType.ATA:
                blankCheck = false;

                return AtaReadBlocks(out buffer, block, count, out duration, out recoveredError);
            case DeviceType.ATAPI:
            case DeviceType.SCSI:
                return ScsiReadBlocks(out buffer, block, count, out duration, out recoveredError, out blankCheck);
            default:
                buffer         = null;
                duration       = 0d;
                recoveredError = false;
                blankCheck     = false;

                return true;
        }
    }

    internal bool ReadChs(out byte[] buffer, ushort cylinder, byte head, byte sector, out double duration,
                          out bool   recoveredError)
    {
        switch(_dev.Type)
        {
            case DeviceType.ATA:
                return AtaReadChs(out buffer, cylinder, head, sector, out duration, out recoveredError);
            default:
                buffer         = null;
                duration       = 0d;
                recoveredError = false;

                return true;
        }
    }

    internal bool Seek(ulong block, out double duration)
    {
        switch(_dev.Type)
        {
            case DeviceType.ATA:
                return AtaSeek(block, out duration);
            case DeviceType.ATAPI:
            case DeviceType.SCSI:
                return ScsiSeek(block, out duration);
            default:
                duration = 0d;

                return true;
        }
    }

    internal bool SeekChs(ushort cylinder, byte head, byte sector, out double duration)
    {
        switch(_dev.Type)
        {
            case DeviceType.ATA:
                return AtaSeekChs(cylinder, head, sector, out duration);
            default:
                duration = 0;

                return true;
        }
    }
}