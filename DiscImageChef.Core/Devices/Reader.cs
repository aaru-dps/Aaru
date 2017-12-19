// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Console;
using DiscImageChef.Devices;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Core.Devices
{
    public partial class Reader
    {
        Device dev;
        uint timeout;
        ulong blocks;
        uint blocksToRead;
        string errorMessage;
        bool readRaw;
        uint blockSize;
        uint physicalsectorsize;
        uint longBlockSize;

        public string ErrorMessage { get { return errorMessage; } }
        public ulong Blocks { get { return blocks; } }
        public uint BlocksToRead { get { return blocksToRead; } }
        public uint LogicalBlockSize { get { return blockSize; } }
        public uint PhysicalBlockSize { get { return physicalsectorsize; } }
        public uint LongBlockSize { get { return longBlockSize; } }
        public bool CanReadRaw { get { return readRaw; } }
        public bool CanSeek { get { return ataSeek || seek6 || seek10; } }
        public bool CanSeekLBA { get { return ataSeekLba || seek6 || seek10; } }

        public Reader(Device dev, uint timeout, byte[] identification, bool raw = false)
        {
            this.dev = dev;
            this.timeout = timeout;
            blocksToRead = 64;
            readRaw = raw;

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    if(Identify.Decode(identification).HasValue)
                        ataId = Identify.Decode(identification).Value;
                    break;
                case DeviceType.NVMe:
                    throw new NotImplementedException("NVMe devices not yet supported.");
            }
        }

        public ulong GetDeviceBlocks()
        {
            switch(dev.Type)
            {
                case DeviceType.ATA:
                    return AtaGetBlocks();
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    return ScsiGetBlocks();
                default:
                    errorMessage = string.Format("Unknown device type {0}.", dev.Type);
                    return 0;
            }
        }

        public bool FindReadCommand()
        {
            switch(dev.Type)
            {
                case DeviceType.ATA:
                    return AtaFindReadCommand();
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    return ScsiFindReadCommand();
                default:
                    errorMessage = string.Format("Unknown device type {0}.", dev.Type);
                    return true;
            }
        }

        public bool GetBlockSize()
        {
            switch(dev.Type)
            {
                case DeviceType.ATA:
                    return AtaGetBlockSize();
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    return ScsiGetBlockSize();
                default:
                    errorMessage = string.Format("Unknown device type {0}.", dev.Type);
                    return true;
            }
        }

        public bool GetBlocksToRead(uint startWithBlocks = 64)
        {
            switch(dev.Type)
            {
                case DeviceType.ATA:
                    return AtaGetBlocksToRead(startWithBlocks);
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    return ScsiGetBlocksToRead(startWithBlocks);
                default:
                    errorMessage = string.Format("Unknown device type {0}.", dev.Type);
                    return true;
            }
        }

        public bool ReadBlock(out byte[] buffer, ulong block, out double duration)
        {
            return ReadBlocks(out buffer, block, 1, out duration);
        }

        public bool ReadBlocks(out byte[] buffer, ulong block, out double duration)
        {
            return ReadBlocks(out buffer, block, blocksToRead, out duration);
        }

        public bool ReadBlocks(out byte[] buffer, ulong block, uint count, out double duration)
        {
            switch(dev.Type)
            {
                case DeviceType.ATA:
                    return AtaReadBlocks(out buffer, block, count, out duration);
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    return ScsiReadBlocks(out buffer, block, count, out duration);
                default:
                    buffer = null;
                    duration = 0d;
                    return true;
            }
        }

        public bool ReadCHS(out byte[] buffer, ushort cylinder, byte head, byte sector, out double duration)
        {
            switch(dev.Type)
            {
                case DeviceType.ATA:
                    return AtaReadCHS(out buffer, cylinder, head, sector, out duration);
                default:
                    buffer = null;
                    duration = 0d;
                    return true;
            }
        }

        public bool Seek(ulong block, out double duration)
        {
            switch(dev.Type)
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

        public bool SeekCHS(ushort cylinder, byte head, byte sector, out double duration)
        {
            switch(dev.Type)
            {
                case DeviceType.ATA:
                    return AtaSeekCHS(cylinder, head, sector, out duration);
                default:
                    duration = 0;
                    return true;
            }
        }
    }
}
