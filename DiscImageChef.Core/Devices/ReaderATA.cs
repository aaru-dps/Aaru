// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices
{
    partial class Reader
    {
        bool ataReadLba;
        bool ataReadRetryLba;
        bool ataReadDmaLba;
        bool ataReadDmaRetryLba;
        bool ataReadLba48;
        bool ataReadDmaLba48;
        bool ataRead;
        bool ataReadRetry;
        bool ataReadDma;
        bool ataReadDmaRetry;
        bool ataSeek;
        bool ataSeekLba;

        Identify.IdentifyDevice ataId;

        internal bool IsLba { get; private set; }
        internal ushort Cylinders { get; private set; }
        internal byte Heads { get; private set; }
        internal byte Sectors { get; private set; }

        (uint, byte, byte) GetDeviceChs()
        {
            if(dev.Type != DeviceType.ATA) return (0, 0, 0);

            if(ataId.CurrentCylinders > 0 && ataId.CurrentHeads > 0 && ataId.CurrentSectorsPerTrack > 0)
            {
                Cylinders = ataId.CurrentCylinders;
                Heads = (byte)ataId.CurrentHeads;
                Sectors = (byte)ataId.CurrentSectorsPerTrack;
                Blocks = (ulong)(Cylinders * Heads * Sectors);
            }

            if((ataId.CurrentCylinders != 0 && ataId.CurrentHeads != 0 && ataId.CurrentSectorsPerTrack != 0) ||
               ataId.Cylinders <= 0 || ataId.Heads <= 0 ||
               ataId.SectorsPerTrack <= 0) return (Cylinders, Heads, Sectors);

            Cylinders = ataId.Cylinders;
            Heads = (byte)ataId.Heads;
            Sectors = (byte)ataId.SectorsPerTrack;
            Blocks = (ulong)(Cylinders * Heads * Sectors);

            return (Cylinders, Heads, Sectors);
        }

        ulong AtaGetBlocks()
        {
            GetDeviceChs();

            if(ataId.Capabilities.HasFlag(Identify.CapabilitiesBit.LBASupport))
            {
                Blocks = ataId.LBASectors;
                IsLba = true;
            }

            if(!ataId.CommandSet2.HasFlag(Identify.CommandSetBit2.LBA48)) return Blocks;

            Blocks = ataId.LBA48Sectors;
            IsLba = true;

            return Blocks;
        }

        bool AtaFindReadCommand()
        {
            byte[] cmdBuf;
            bool sense;
            AtaErrorRegistersCHS errorChs;
            AtaErrorRegistersLBA28 errorLba;
            AtaErrorRegistersLBA48 errorLba48;
            double duration;

            sense = dev.Read(out cmdBuf, out errorChs, false, 0, 0, 1, 1, timeout, out duration);
            ataRead = !sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0;
            sense = dev.Read(out cmdBuf, out errorChs, true, 0, 0, 1, 1, timeout, out duration);
            ataReadRetry = !sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0;
            sense = dev.ReadDma(out cmdBuf, out errorChs, false, 0, 0, 1, 1, timeout, out duration);
            ataReadDma = !sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0;
            sense = dev.ReadDma(out cmdBuf, out errorChs, true, 0, 0, 1, 1, timeout, out duration);
            ataReadDmaRetry = !sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0;

            sense = dev.Read(out cmdBuf, out errorLba, false, 0, 1, timeout, out duration);
            ataReadLba = !sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0;
            sense = dev.Read(out cmdBuf, out errorLba, true, 0, 1, timeout, out duration);
            ataReadRetryLba = !sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0;
            sense = dev.ReadDma(out cmdBuf, out errorLba, false, 0, 1, timeout, out duration);
            ataReadDmaLba = !sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0;
            sense = dev.ReadDma(out cmdBuf, out errorLba, true, 0, 1, timeout, out duration);
            ataReadDmaRetryLba = !sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0;

            sense = dev.Read(out cmdBuf, out errorLba48, 0, 1, timeout, out duration);
            ataReadLba48 = !sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0;
            sense = dev.ReadDma(out cmdBuf, out errorLba48, 0, 1, timeout, out duration);
            ataReadDmaLba48 = !sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0;

            sense = dev.Seek(out errorChs, 0, 0, 1, timeout, out duration);
            ataSeek = !sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0;
            sense = dev.Seek(out errorLba, 0, timeout, out duration);
            ataSeekLba = !sense && (errorLba.status & 0x27) == 0 && errorChs.error == 0;

            if(IsLba)
            {
                if(Blocks > 0xFFFFFFF && !ataReadLba48 && !ataReadDmaLba48)
                {
                    ErrorMessage = "Device needs 48-bit LBA commands but I can't issue them... Aborting.";
                    return true;
                }

                if(!ataReadLba && !ataReadRetryLba && !ataReadDmaLba && !ataReadDmaRetryLba)
                {
                    ErrorMessage = "Device needs 28-bit LBA commands but I can't issue them... Aborting.";
                    return true;
                }
            }
            else
            {
                if(!ataRead && !ataReadRetry && !ataReadDma && !ataReadDmaRetry)
                {
                    ErrorMessage = "Device needs CHS commands but I can't issue them... Aborting.";
                    return true;
                }
            }

            if(ataReadDmaLba48) DicConsole.WriteLine("Using ATA READ DMA EXT command.");
            else if(ataReadLba48) DicConsole.WriteLine("Using ATA READ EXT command.");
            else if(ataReadDmaRetryLba) DicConsole.WriteLine("Using ATA READ DMA command with retries (LBA).");
            else if(ataReadDmaLba) DicConsole.WriteLine("Using ATA READ DMA command (LBA).");
            else if(ataReadRetryLba) DicConsole.WriteLine("Using ATA READ command with retries (LBA).");
            else if(ataReadLba) DicConsole.WriteLine("Using ATA READ command (LBA).");
            else if(ataReadDmaRetry) DicConsole.WriteLine("Using ATA READ DMA command with retries (CHS).");
            else if(ataReadDma) DicConsole.WriteLine("Using ATA READ DMA command (CHS).");
            else if(ataReadRetry) DicConsole.WriteLine("Using ATA READ command with retries (CHS).");
            else if(ataRead) DicConsole.WriteLine("Using ATA READ command (CHS).");
            else
            {
                ErrorMessage = "Could not get a working read command!";
                return true;
            }

            return false;
        }

        bool AtaGetBlockSize()
        {
            if((ataId.PhysLogSectorSize & 0x8000) == 0x0000 && (ataId.PhysLogSectorSize & 0x4000) == 0x4000)
            {
                if((ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                    if(ataId.LogicalSectorWords <= 255 || ataId.LogicalAlignment == 0xFFFF) LogicalBlockSize = 512;
                    else LogicalBlockSize = ataId.LogicalSectorWords * 2;
                else LogicalBlockSize = 512;

                if((ataId.PhysLogSectorSize & 0x2000) == 0x2000)
                {
                    PhysicalBlockSize = LogicalBlockSize * (uint)Math.Pow(2, ataId.PhysLogSectorSize & 0xF);
                }
                else PhysicalBlockSize = LogicalBlockSize;
            }
            else
            {
                LogicalBlockSize = 512;
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

            byte[] cmdBuf;
            bool sense;
            AtaErrorRegistersLBA28 errorLba;
            AtaErrorRegistersLBA48 errorLba48;
            double duration;
            bool error = true;

            while(IsLba)
            {
                if(ataReadDmaLba48)
                {
                    sense = dev.ReadDma(out cmdBuf, out errorLba48, 0, (byte)BlocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                }
                else if(ataReadLba48)
                {
                    sense = dev.Read(out cmdBuf, out errorLba48, 0, (byte)BlocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                }
                else if(ataReadDmaRetryLba)
                {
                    sense = dev.ReadDma(out cmdBuf, out errorLba, true, 0, (byte)BlocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                }
                else if(ataReadDmaLba)
                {
                    sense = dev.ReadDma(out cmdBuf, out errorLba, false, 0, (byte)BlocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                }
                else if(ataReadRetryLba)
                {
                    sense = dev.Read(out cmdBuf, out errorLba, true, 0, (byte)BlocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                }
                else if(ataReadLba)
                {
                    sense = dev.Read(out cmdBuf, out errorLba, false, 0, (byte)BlocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                }

                if(error) BlocksToRead /= 2;

                if(!error || BlocksToRead == 1) break;
            }

            if(!error || !IsLba) return false;

            BlocksToRead = 1;
            ErrorMessage = $"Device error {dev.LastError} trying to guess ideal transfer length.";
            return true;
        }

        bool AtaReadBlocks(out byte[] buffer, ulong block, uint count, out double duration)
        {
            bool error = true;
            bool sense;
            AtaErrorRegistersLBA28 errorLba;
            AtaErrorRegistersLBA48 errorLba48;
            byte status = 0, errorByte = 0;
            buffer = null;
            duration = 0;

            if(ataReadDmaLba48)
            {
                sense = dev.ReadDma(out buffer, out errorLba48, block, (byte)count, timeout, out duration);
                error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && buffer.Length > 0);
                status = errorLba48.status;
                errorByte = errorLba48.error;
            }
            else if(ataReadLba48)
            {
                sense = dev.Read(out buffer, out errorLba48, block, (byte)count, timeout, out duration);
                error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && buffer.Length > 0);
                status = errorLba48.status;
                errorByte = errorLba48.error;
            }
            else if(ataReadDmaRetryLba)
            {
                sense = dev.ReadDma(out buffer, out errorLba, true, (uint)block, (byte)count, timeout, out duration);
                error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && buffer.Length > 0);
                status = errorLba.status;
                errorByte = errorLba.error;
            }
            else if(ataReadDmaLba)
            {
                sense = dev.ReadDma(out buffer, out errorLba, false, (uint)block, (byte)count, timeout, out duration);
                error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && buffer.Length > 0);
                status = errorLba.status;
                errorByte = errorLba.error;
            }
            else if(ataReadRetryLba)
            {
                sense = dev.Read(out buffer, out errorLba, true, (uint)block, (byte)count, timeout, out duration);
                error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && buffer.Length > 0);
                status = errorLba.status;
                errorByte = errorLba.error;
            }
            else if(ataReadLba)
            {
                sense = dev.Read(out buffer, out errorLba, false, (uint)block, (byte)count, timeout, out duration);
                error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && buffer.Length > 0);
                status = errorLba.status;
                errorByte = errorLba.error;
            }

            if(error) DicConsole.DebugWriteLine("ATA Reader", "ATA ERROR: {0} STATUS: {1}", errorByte, status);

            return error;
        }

        bool AtaReadChs(out byte[] buffer, ushort cylinder, byte head, byte sectir, out double duration)
        {
            bool error = true;
            bool sense;
            AtaErrorRegistersCHS errorChs;
            byte status = 0, errorByte = 0;
            buffer = null;
            duration = 0;

            if(ataReadDmaRetry)
            {
                sense = dev.ReadDma(out buffer, out errorChs, true, cylinder, head, sectir, 1, timeout, out duration);
                error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && buffer.Length > 0);
                status = errorChs.status;
                errorByte = errorChs.error;
            }
            else if(ataReadDma)
            {
                sense = dev.ReadDma(out buffer, out errorChs, false, cylinder, head, sectir, 1, timeout, out duration);
                error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && buffer.Length > 0);
                status = errorChs.status;
                errorByte = errorChs.error;
            }
            else if(ataReadRetry)
            {
                sense = dev.Read(out buffer, out errorChs, true, cylinder, head, sectir, 1, timeout, out duration);
                error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && buffer.Length > 0);
                status = errorChs.status;
                errorByte = errorChs.error;
            }
            else if(ataRead)
            {
                sense = dev.Read(out buffer, out errorChs, false, cylinder, head, sectir, 1, timeout, out duration);
                error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && buffer.Length > 0);
                status = errorChs.status;
                errorByte = errorChs.error;
            }

            if(error) DicConsole.DebugWriteLine("ATA Reader", "ATA ERROR: {0} STATUS: {1}", errorByte, status);

            return error;
        }

        bool AtaSeek(ulong block, out double duration)
        {
            AtaErrorRegistersLBA28 errorLba;

            bool sense = dev.Seek(out errorLba, (uint)block, timeout, out duration);
            return !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0);
        }

        bool AtaSeekChs(ushort cylinder, byte head, byte sector, out double duration)
        {
            AtaErrorRegistersCHS errorChs;

            bool sense = dev.Seek(out errorChs, cylinder, head, sector, timeout, out duration);
            return !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0);
        }
    }
}