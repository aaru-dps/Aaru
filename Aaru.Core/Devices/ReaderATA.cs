// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ReaderATA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains common code for reading ATA devices.
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
using Aaru.Console;
using Aaru.Decoders.ATA;
using Identify = Aaru.CommonTypes.Structs.Devices.ATA.Identify;

namespace Aaru.Core.Devices;

sealed partial class Reader
{
    Identify.IdentifyDevice _ataId;
    bool                    _ataRead;
    bool                    _ataReadDma;
    bool                    _ataReadDmaLba;
    bool                    _ataReadDmaLba48;
    bool                    _ataReadDmaRetry;
    bool                    _ataReadDmaRetryLba;
    bool                    _ataReadLba;
    bool                    _ataReadLba48;
    bool                    _ataReadRetry;
    bool                    _ataReadRetryLba;
    bool                    _ataSeek;
    bool                    _ataSeekLba;

    internal bool   IsLba     { get; private set; }
    internal ushort Cylinders { get; private set; }
    internal byte   Heads     { get; private set; }
    internal byte   Sectors   { get; private set; }

    void GetDeviceChs()
    {
        if(_dev.Type != DeviceType.ATA)
            return;

        if(_ataId.CurrentCylinders > 0 &&
           _ataId is { CurrentHeads: > 0, CurrentSectorsPerTrack: > 0 })
        {
            Cylinders = _ataId.CurrentCylinders;
            Heads     = (byte)_ataId.CurrentHeads;
            Sectors   = (byte)_ataId.CurrentSectorsPerTrack;
            Blocks    = (ulong)(Cylinders * Heads * Sectors);
        }

        if((_ataId.CurrentCylinders != 0 && _ataId.CurrentHeads != 0 && _ataId.CurrentSectorsPerTrack != 0) ||
           _ataId.Cylinders       <= 0                                                                      ||
           _ataId.Heads           <= 0                                                                      ||
           _ataId.SectorsPerTrack <= 0)
            return;

        Cylinders = _ataId.Cylinders;
        Heads     = (byte)_ataId.Heads;
        Sectors   = (byte)_ataId.SectorsPerTrack;
        Blocks    = (ulong)(Cylinders * Heads * Sectors);
    }

    ulong AtaGetBlocks()
    {
        GetDeviceChs();

        if(_ataId.Capabilities.HasFlag(Identify.CapabilitiesBit.LBASupport))
        {
            Blocks = _ataId.LBASectors;
            IsLba  = true;
        }

        if(!_ataId.CommandSet2.HasFlag(Identify.CommandSetBit2.LBA48))
            return Blocks;

        Blocks = _ataId.LBA48Sectors;
        IsLba  = true;

        return Blocks;
    }

    bool AtaFindReadCommand()
    {
        if(Blocks == 0)
            GetDeviceBlocks();

        bool                   sense;
        int                    tries  = 0;
        uint                   lba    = 0;
        ushort                 cyl    = 0;
        byte                   head   = 0;
        byte                   sector = 1;
        AtaErrorRegistersChs   errorChs;
        AtaErrorRegistersLba28 errorLba;
        var                    rnd = new Random();

        while(tries < 10)
        {
            sense            = _dev.Read(out byte[] cmdBuf, out errorChs, false, cyl, head, sector, 1, _timeout, out _);
            _ataRead         = !sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0 && cmdBuf.Length > 0;
            sense            = _dev.Read(out cmdBuf, out errorChs, true, cyl, head, sector, 1, _timeout, out _);
            _ataReadRetry    = !sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0 && cmdBuf.Length > 0;
            sense            = _dev.ReadDma(out cmdBuf, out errorChs, false, cyl, head, sector, 1, _timeout, out _);
            _ataReadDma      = !sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0 && cmdBuf.Length > 0;
            sense            = _dev.ReadDma(out cmdBuf, out errorChs, true, cyl, head, sector, 1, _timeout, out _);
            _ataReadDmaRetry = !sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0 && cmdBuf.Length > 0;

            sense            = _dev.Read(out cmdBuf, out errorLba, false, lba, 1, _timeout, out _);
            _ataReadLba      = !sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && cmdBuf.Length > 0;
            sense            = _dev.Read(out cmdBuf, out errorLba, true, lba, 1, _timeout, out _);
            _ataReadRetryLba = !sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && cmdBuf.Length > 0;
            sense            = _dev.ReadDma(out cmdBuf, out errorLba, false, lba, 1, _timeout, out _);
            _ataReadDmaLba   = !sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && cmdBuf.Length > 0;
            sense            = _dev.ReadDma(out cmdBuf, out errorLba, true, lba, 1, _timeout, out _);

            _ataReadDmaRetryLba = !sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && cmdBuf.Length > 0;

            sense         = _dev.Read(out cmdBuf, out AtaErrorRegistersLba48 errorLba48, lba, 1, _timeout, out _);
            _ataReadLba48 = !sense && (errorLba48.Status & 0x27) == 0 && errorLba48.Error == 0 && cmdBuf.Length > 0;
            sense         = _dev.ReadDma(out cmdBuf, out errorLba48, lba, 1, _timeout, out _);

            _ataReadDmaLba48 = !sense && (errorLba48.Status & 0x27) == 0 && errorLba48.Error == 0 && cmdBuf.Length > 0;

            if(_ataRead            ||
               _ataReadRetry       ||
               _ataReadDma         ||
               _ataReadDmaRetry    ||
               _ataReadLba         ||
               _ataReadRetryLba    ||
               _ataReadDmaLba      ||
               _ataReadDmaRetryLba ||
               _ataReadLba48       ||
               _ataReadDmaLba48)
                break;

            lba    = (uint)rnd.Next(1, (int)Blocks);
            cyl    = (ushort)rnd.Next(0, Cylinders);
            head   = (byte)rnd.Next(0, Heads);
            sector = (byte)rnd.Next(1, Sectors);
            tries++;
        }

        sense       = _dev.Seek(out errorChs, 0, 0, 1, _timeout, out _);
        _ataSeek    = !sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0;
        sense       = _dev.Seek(out errorLba, 0, _timeout, out _);
        _ataSeekLba = !sense && (errorLba.Status & 0x27) == 0 && errorChs.Error == 0;

        if(IsLba)
        {
            if(Blocks > 0xFFFFFFF &&
               !_ataReadLba48     &&
               !_ataReadDmaLba48)
            {
                ErrorMessage = Localization.Core.Device_needs_48_bit_LBA_commands_but_I_cant_issue_them_Aborting;

                return true;
            }

            if(!_ataReadLba      &&
               !_ataReadRetryLba &&
               !_ataReadDmaLba   &&
               !_ataReadDmaRetryLba)
            {
                ErrorMessage = Localization.Core.Device_needs_28_bit_LBA_commands_but_I_cant_issue_them_Aborting;

                return true;
            }
        }
        else
        {
            if(!_ataRead      &&
               !_ataReadRetry &&
               !_ataReadDma   &&
               !_ataReadDmaRetry)
            {
                ErrorMessage = Localization.Core.Device_needs_CHS_commands_but_I_cant_issue_them_Aborting;

                return true;
            }
        }

        if(_ataReadDmaLba48)
            AaruConsole.WriteLine(Localization.Core.Using_ATA_READ_DMA_EXT_command);
        else if(_ataReadLba48)
            AaruConsole.WriteLine(Localization.Core.Using_ATA_READ_EXT_command);
        else if(_ataReadDmaRetryLba)
            AaruConsole.WriteLine(Localization.Core.Using_ATA_READ_DMA_command_with_retries_LBA);
        else if(_ataReadDmaLba)
            AaruConsole.WriteLine(Localization.Core.Using_ATA_READ_DMA_command_LBA);
        else if(_ataReadRetryLba)
            AaruConsole.WriteLine(Localization.Core.Using_ATA_READ_command_with_retries_LBA);
        else if(_ataReadLba)
            AaruConsole.WriteLine(Localization.Core.Using_ATA_READ_command_LBA);
        else if(_ataReadDmaRetry)
            AaruConsole.WriteLine(Localization.Core.Using_ATA_READ_DMA_command_with_retries_CHS);
        else if(_ataReadDma)
            AaruConsole.WriteLine(Localization.Core.Using_ATA_READ_DMA_command_CHS);
        else if(_ataReadRetry)
            AaruConsole.WriteLine(Localization.Core.Using_ATA_READ_command_with_retries_CHS);
        else if(_ataRead)
            AaruConsole.WriteLine(Localization.Core.Using_ATA_READ_command_CHS);
        else
        {
            ErrorMessage = Localization.Core.Could_not_get_a_working_read_command;

            return true;
        }

        return false;
    }

    bool AtaGetBlockSize()
    {
        if((_ataId.PhysLogSectorSize & 0x8000) == 0x0000 &&
           (_ataId.PhysLogSectorSize & 0x4000) == 0x4000)
        {
            if((_ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                if(_ataId.LogicalSectorWords <= 255 ||
                   _ataId.LogicalAlignment   == 0xFFFF)
                    LogicalBlockSize = 512;
                else
                    LogicalBlockSize = _ataId.LogicalSectorWords * 2;
            else
                LogicalBlockSize = 512;

            if((_ataId.PhysLogSectorSize & 0x2000) == 0x2000)
                PhysicalBlockSize = LogicalBlockSize * (uint)Math.Pow(2, _ataId.PhysLogSectorSize & 0xF);
            else
                PhysicalBlockSize = LogicalBlockSize;
        }
        else
        {
            LogicalBlockSize  = 512;
            PhysicalBlockSize = 512;
        }

        // TODO: ATA READ LONG
        LongBlockSize = 0;

        return false;
    }

    bool AtaGetBlocksToRead(uint startWithBlocks)
    {
        BlocksToRead = startWithBlocks;

        if(!IsLba)
        {
            BlocksToRead = 1;

            return false;
        }

        bool error = true;

        while(IsLba)
        {
            byte[]                 cmdBuf;
            bool                   sense;
            AtaErrorRegistersLba48 errorLba48;

            if(_ataReadDmaLba48)
            {
                sense = _dev.ReadDma(out cmdBuf, out errorLba48, 0, (byte)BlocksToRead, _timeout, out _);
                error = !(!sense && (errorLba48.Status & 0x27) == 0 && errorLba48.Error == 0 && cmdBuf.Length > 0);
            }
            else if(_ataReadLba48)
            {
                sense = _dev.Read(out cmdBuf, out errorLba48, 0, (byte)BlocksToRead, _timeout, out _);
                error = !(!sense && (errorLba48.Status & 0x27) == 0 && errorLba48.Error == 0 && cmdBuf.Length > 0);
            }
            else
            {
                AtaErrorRegistersLba28 errorLba;

                if(_ataReadDmaRetryLba)
                {
                    sense = _dev.ReadDma(out cmdBuf, out errorLba, true, 0, (byte)BlocksToRead, _timeout, out _);
                    error = !(!sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && cmdBuf.Length > 0);
                }
                else if(_ataReadDmaLba)
                {
                    sense = _dev.ReadDma(out cmdBuf, out errorLba, false, 0, (byte)BlocksToRead, _timeout, out _);
                    error = !(!sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && cmdBuf.Length > 0);
                }
                else if(_ataReadRetryLba)
                {
                    sense = _dev.Read(out cmdBuf, out errorLba, true, 0, (byte)BlocksToRead, _timeout, out _);
                    error = !(!sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && cmdBuf.Length > 0);
                }
                else if(_ataReadLba)
                {
                    sense = _dev.Read(out cmdBuf, out errorLba, false, 0, (byte)BlocksToRead, _timeout, out _);
                    error = !(!sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && cmdBuf.Length > 0);
                }
            }

            if(error)
                BlocksToRead /= 2;

            if(!error ||
               BlocksToRead == 1)
                break;
        }

        if(!error ||
           !IsLba)
            return false;

        BlocksToRead = 1;

        ErrorMessage = string.Format(Localization.Core.Device_error_0_trying_to_guess_ideal_transfer_length,
                                     _dev.LastError);

        return true;
    }

    bool AtaReadBlocks(out byte[] buffer, ulong block, uint count, out double duration, out bool recoveredError)
    {
        bool                   error = true;
        bool                   sense;
        AtaErrorRegistersLba28 errorLba;
        AtaErrorRegistersLba48 errorLba48;
        byte                   status = 0, errorByte = 0;
        buffer         = null;
        duration       = 0;
        recoveredError = false;

        if(_ataReadDmaLba48)
        {
            sense     = _dev.ReadDma(out buffer, out errorLba48, block, (byte)count, _timeout, out duration);
            error     = !(!sense && (errorLba48.Status & 0x27) == 0 && errorLba48.Error == 0 && buffer.Length > 0);
            status    = errorLba48.Status;
            errorByte = errorLba48.Error;

            if(error)
                _errorLog?.WriteLine(block, _dev.Error, _dev.LastError, errorLba48);
        }
        else if(_ataReadLba48)
        {
            sense     = _dev.Read(out buffer, out errorLba48, block, (byte)count, _timeout, out duration);
            error     = !(!sense && (errorLba48.Status & 0x27) == 0 && errorLba48.Error == 0 && buffer.Length > 0);
            status    = errorLba48.Status;
            errorByte = errorLba48.Error;

            if(error)
                _errorLog?.WriteLine(block, _dev.Error, _dev.LastError, errorLba48);
        }
        else if(_ataReadDmaRetryLba)
        {
            sense = _dev.ReadDma(out buffer, out errorLba, true, (uint)block, (byte)count, _timeout, out duration);

            error     = !(!sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && buffer.Length > 0);
            status    = errorLba.Status;
            errorByte = errorLba.Error;

            if(error)
                _errorLog?.WriteLine(block, _dev.Error, _dev.LastError, errorLba);
        }
        else if(_ataReadDmaLba)
        {
            sense = _dev.ReadDma(out buffer, out errorLba, false, (uint)block, (byte)count, _timeout, out duration);

            error     = !(!sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && buffer.Length > 0);
            status    = errorLba.Status;
            errorByte = errorLba.Error;

            if(error)
                _errorLog?.WriteLine(block, _dev.Error, _dev.LastError, errorLba);
        }
        else if(_ataReadRetryLba)
        {
            sense     = _dev.Read(out buffer, out errorLba, true, (uint)block, (byte)count, _timeout, out duration);
            error     = !(!sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && buffer.Length > 0);
            status    = errorLba.Status;
            errorByte = errorLba.Error;

            if(error)
                _errorLog?.WriteLine(block, _dev.Error, _dev.LastError, errorLba);
        }
        else if(_ataReadLba)
        {
            sense     = _dev.Read(out buffer, out errorLba, false, (uint)block, (byte)count, _timeout, out duration);
            error     = !(!sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0 && buffer.Length > 0);
            status    = errorLba.Status;
            errorByte = errorLba.Error;

            if(error)
                _errorLog?.WriteLine(block, _dev.Error, _dev.LastError, errorLba);
        }

        if(!error)
            return false;

        if((status & 0x04) == 0x04)
            recoveredError = true;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.Core.ATA_ERROR_0_STATUS_1, errorByte, status);

        return true;
    }

    bool AtaReadChs(out byte[] buffer, ushort cylinder, byte head, byte sector, out double duration,
                    out bool recoveredError)
    {
        bool                 error = true;
        bool                 sense;
        AtaErrorRegistersChs errorChs;
        byte                 status = 0, errorByte = 0;
        buffer         = null;
        duration       = 0;
        recoveredError = false;

        if(_ataReadDmaRetry)
        {
            sense = _dev.ReadDma(out buffer, out errorChs, true, cylinder, head, sector, 1, _timeout, out duration);

            error     = !(!sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0 && buffer.Length > 0);
            status    = errorChs.Status;
            errorByte = errorChs.Error;

            if(error)
                _errorLog?.WriteLine(cylinder, head, sector, _dev.Error, _dev.LastError, errorChs);
        }
        else if(_ataReadDma)
        {
            sense = _dev.ReadDma(out buffer, out errorChs, false, cylinder, head, sector, 1, _timeout, out duration);

            error     = !(!sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0 && buffer.Length > 0);
            status    = errorChs.Status;
            errorByte = errorChs.Error;

            if(error)
                _errorLog?.WriteLine(cylinder, head, sector, _dev.Error, _dev.LastError, errorChs);
        }
        else if(_ataReadRetry)
        {
            sense     = _dev.Read(out buffer, out errorChs, true, cylinder, head, sector, 1, _timeout, out duration);
            error     = !(!sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0 && buffer.Length > 0);
            status    = errorChs.Status;
            errorByte = errorChs.Error;

            if(error)
                _errorLog?.WriteLine(cylinder, head, sector, _dev.Error, _dev.LastError, errorChs);
        }
        else if(_ataRead)
        {
            sense     = _dev.Read(out buffer, out errorChs, false, cylinder, head, sector, 1, _timeout, out duration);
            error     = !(!sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0 && buffer.Length > 0);
            status    = errorChs.Status;
            errorByte = errorChs.Error;

            if(error)
                _errorLog?.WriteLine(cylinder, head, sector, _dev.Error, _dev.LastError, errorChs);
        }

        if(!error)
            return false;

        if((status & 0x04) == 0x04)
            recoveredError = true;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.Core.ATA_ERROR_0_STATUS_1, errorByte, status);

        return true;
    }

    bool AtaSeek(ulong block, out double duration)
    {
        bool sense = _dev.Seek(out AtaErrorRegistersLba28 errorLba, (uint)block, _timeout, out duration);

        if(!(!sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0))
            _errorLog?.WriteLine(block, _dev.Error, _dev.LastError, errorLba);

        return !(!sense && (errorLba.Status & 0x27) == 0 && errorLba.Error == 0);
    }

    bool AtaSeekChs(ushort cylinder, byte head, byte sector, out double duration)
    {
        bool sense = _dev.Seek(out AtaErrorRegistersChs errorChs, cylinder, head, sector, _timeout, out duration);

        if(!(!sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0))
            _errorLog?.WriteLine(cylinder, head, sector, _dev.Error, _dev.LastError, errorChs);

        return !(!sense && (errorChs.Status & 0x27) == 0 && errorChs.Error == 0);
    }
}