// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ReaderATA.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices
{
    public partial class Reader
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
        bool lbaMode;
        ushort cylinders;
        byte heads, sectors;
        bool ataSeek;
        bool ataSeekLba;

        Identify.IdentifyDevice ataId;

        public bool IsLBA { get { return lbaMode; } }
        public ushort Cylinders { get { return cylinders; } }
        public byte Heads { get { return heads; } }
        public byte Sectors { get { return sectors; } }

        public (uint, byte, byte) GetDeviceCHS()
        {
            if(dev.Type != DeviceType.ATA)
                return (0, 0, 0);

            if(ataId.CurrentCylinders > 0 && ataId.CurrentHeads > 0 && ataId.CurrentSectorsPerTrack > 0)
            {
                cylinders = ataId.CurrentCylinders;
                heads = (byte)ataId.CurrentHeads;
                sectors = (byte)ataId.CurrentSectorsPerTrack;
                blocks = (ulong)(cylinders * heads * sectors);
            }

            if((ataId.CurrentCylinders == 0 || ataId.CurrentHeads == 0 || ataId.CurrentSectorsPerTrack == 0) &&
                (ataId.Cylinders > 0 && ataId.Heads > 0 && ataId.SectorsPerTrack > 0))
            {
                cylinders = ataId.Cylinders;
                heads = (byte)ataId.Heads;
                sectors = (byte)ataId.SectorsPerTrack;
                blocks = (ulong)(cylinders * heads * sectors);
            }

            return (cylinders, heads, sectors);
        }

        ulong AtaGetBlocks()
        {
            GetDeviceCHS();

            if(ataId.Capabilities.HasFlag(Identify.CapabilitiesBit.LBASupport))
            {
                blocks = ataId.LBASectors;
                lbaMode = true;
            }

            if(ataId.CommandSet2.HasFlag(Identify.CommandSetBit2.LBA48))
            {
                blocks = ataId.LBA48Sectors;
                lbaMode = true;
            }

            return blocks;
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
            ataRead = (!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
            sense = dev.Read(out cmdBuf, out errorChs, true, 0, 0, 1, 1, timeout, out duration);
            ataReadRetry = (!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
            sense = dev.ReadDma(out cmdBuf, out errorChs, false, 0, 0, 1, 1, timeout, out duration);
            ataReadDma = (!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
            sense = dev.ReadDma(out cmdBuf, out errorChs, true, 0, 0, 1, 1, timeout, out duration);
            ataReadDmaRetry = (!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);

            sense = dev.Read(out cmdBuf, out errorLba, false, 0, 1, timeout, out duration);
            ataReadLba = (!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
            sense = dev.Read(out cmdBuf, out errorLba, true, 0, 1, timeout, out duration);
            ataReadRetryLba = (!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
            sense = dev.ReadDma(out cmdBuf, out errorLba, false, 0, 1, timeout, out duration);
            ataReadDmaLba = (!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
            sense = dev.ReadDma(out cmdBuf, out errorLba, true, 0, 1, timeout, out duration);
            ataReadDmaRetryLba = (!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);

            sense = dev.Read(out cmdBuf, out errorLba48, 0, 1, timeout, out duration);
            ataReadLba48 = (!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
            sense = dev.ReadDma(out cmdBuf, out errorLba48, 0, 1, timeout, out duration);
            ataReadDmaLba48 = (!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);

            sense = dev.Seek(out errorChs, 0, 0, 1, timeout, out duration);
            ataSeek = (!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0);
            sense = dev.Seek(out errorLba, 0, timeout, out duration);
            ataSeekLba = (!sense && (errorLba.status & 0x27) == 0 && errorChs.error == 0);

            if(!lbaMode)
            {
                if(blocks > 0xFFFFFFF && !ataReadLba48 && !ataReadDmaLba48)
                {
                    errorMessage = "Device needs 48-bit LBA commands but I can't issue them... Aborting.";
                    return true;
                }

                if(!ataReadLba && !ataReadRetryLba && !ataReadDmaLba && !ataReadDmaRetryLba)
                {
                    errorMessage = "Device needs 28-bit LBA commands but I can't issue them... Aborting.";
                    return true;
                }
            }
            else
            {
                if(!ataRead && !ataReadRetry && !ataReadDma && !ataReadDmaRetry)
                {
                    errorMessage = "Device needs CHS commands but I can't issue them... Aborting.";
                    return true;
                }
            }

            if(ataReadDmaLba48)
                DicConsole.WriteLine("Using ATA READ DMA EXT command.");
            else if(ataReadLba48)
                DicConsole.WriteLine("Using ATA READ EXT command.");
            else if(ataReadDmaRetryLba)
                DicConsole.WriteLine("Using ATA READ DMA command with retries (LBA).");
            else if(ataReadDmaLba)
                DicConsole.WriteLine("Using ATA READ DMA command (LBA).");
            else if(ataReadRetryLba)
                DicConsole.WriteLine("Using ATA READ command with retries (LBA).");
            else if(ataReadLba)
                DicConsole.WriteLine("Using ATA READ command (LBA).");
            else if(ataReadDmaRetry)
                DicConsole.WriteLine("Using ATA READ DMA command with retries (CHS).");
            else if(ataReadDma)
                DicConsole.WriteLine("Using ATA READ DMA command (CHS).");
            else if(ataReadRetry)
                DicConsole.WriteLine("Using ATA READ command with retries (CHS).");
            else if(ataRead)
                DicConsole.WriteLine("Using ATA READ command (CHS).");
            else
            {
                errorMessage = "Could not get a working read command!";
                return true;
            }

            return false;
        }

        bool AtaGetBlockSize()
        {
            if((ataId.PhysLogSectorSize & 0x8000) == 0x0000 &&
                                (ataId.PhysLogSectorSize & 0x4000) == 0x4000)
            {
                if((ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                {
                    if(ataId.LogicalSectorWords <= 255 || ataId.LogicalAlignment == 0xFFFF)
                        blockSize = 512;
                    else
                        blockSize = ataId.LogicalSectorWords * 2;
                }
                else
                    blockSize = 512;

                if((ataId.PhysLogSectorSize & 0x2000) == 0x2000)
                {
#pragma warning disable IDE0004 // Cast is necessary, otherwise incorrect value is created
                    physicalsectorsize = blockSize * (uint)Math.Pow(2, (double)(ataId.PhysLogSectorSize & 0xF));
#pragma warning restore IDE0004 // Cast is necessary, otherwise incorrect value is created
                }
                else
                    physicalsectorsize = blockSize;
            }
            else
            {
                blockSize = 512;
                physicalsectorsize = 512;
            }

            // TODO: ATA READ LONG
            longBlockSize = 0;

            return false;
        }

        bool AtaGetBlocksToRead(uint startWithBlocks)
        {
            blocksToRead = startWithBlocks;

            if(!lbaMode)
            {
                blocksToRead = 1;
                return false;
            }

            byte[] cmdBuf;
            bool sense;
            AtaErrorRegistersLBA28 errorLba;
            AtaErrorRegistersLBA48 errorLba48;
            double duration;
            bool error = true;

            while(lbaMode)
            {
                if(ataReadDmaLba48)
                {
                    sense = dev.ReadDma(out cmdBuf, out errorLba48, 0, (byte)blocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                }
                else if(ataReadLba48)
                {
                    sense = dev.Read(out cmdBuf, out errorLba48, 0, (byte)blocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                }
                else if(ataReadDmaRetryLba)
                {
                    sense = dev.ReadDma(out cmdBuf, out errorLba, true, 0, (byte)blocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                }
                else if(ataReadDmaLba)
                {
                    sense = dev.ReadDma(out cmdBuf, out errorLba, false, 0, (byte)blocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                }
                else if(ataReadRetryLba)
                {
                    sense = dev.Read(out cmdBuf, out errorLba, true, 0, (byte)blocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                }
                else if(ataReadLba)
                {
                    sense = dev.Read(out cmdBuf, out errorLba, false, 0, (byte)blocksToRead, timeout, out duration);
                    error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                }

                if(error)
                    blocksToRead /= 2;

                if(!error || blocksToRead == 1)
                    break;
            }

            if(error && lbaMode)
            {
                blocksToRead = 1;
                errorMessage=string.Format("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                return true;
            }

            return false;
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

            if(error)
                DicConsole.DebugWriteLine("ATA Reader", "ATA ERROR: {0} STATUS: {1}", errorByte, status);

            return error;
        }

        bool AtaReadCHS(out byte[] buffer, ushort cylinder, byte head, byte sectir, out double duration)
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

            if(error)
                DicConsole.DebugWriteLine("ATA Reader", "ATA ERROR: {0} STATUS: {1}", errorByte, status);

            return error;
        }

        bool AtaSeek(ulong block, out double duration)
        {
            AtaErrorRegistersLBA28 errorLba;

            bool sense = dev.Seek(out errorLba, (uint)block, timeout, out duration);
            return !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0);
        }

        bool AtaSeekCHS(ushort cylinder, byte head, byte sector, out double duration)
        {
            AtaErrorRegistersCHS errorChs;

            bool sense = dev.Seek(out errorChs, cylinder, head, sector, timeout, out duration);
            return !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0);
        }
    }
}
