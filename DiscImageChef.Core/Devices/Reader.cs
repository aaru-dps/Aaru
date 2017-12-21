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
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices
{
    partial class Reader
    {
        Device dev;
        uint timeout;

        internal string ErrorMessage { get; private set; }
        internal ulong Blocks { get; private set; }
        internal uint BlocksToRead { get; private set; }
        internal uint LogicalBlockSize { get; private set; }
        internal uint PhysicalBlockSize { get; private set; }
        internal uint LongBlockSize { get; private set; }
        internal bool CanReadRaw { get; private set; }
        internal bool CanSeek
        {
            get => ataSeek || seek6 || seek10;
        }
        internal bool CanSeekLba
        {
            get => ataSeekLba || seek6 || seek10;
        }

        internal Reader(Device dev, uint timeout, byte[] identification, bool raw = false)
        {
            this.dev = dev;
            this.timeout = timeout;
            BlocksToRead = 64;
            CanReadRaw = raw;

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    Identify.IdentifyDevice? ataIdNullable = Identify.Decode(identification);
                    if(ataIdNullable.HasValue) ataId = ataIdNullable.Value;
                    break;
                case DeviceType.NVMe: throw new NotImplementedException("NVMe devices not yet supported.");
            }
        }

        internal ulong GetDeviceBlocks()
        {
            switch(dev.Type)
            {
                case DeviceType.ATA: return AtaGetBlocks();
                case DeviceType.ATAPI:
                case DeviceType.SCSI: return ScsiGetBlocks();
                default:
                    ErrorMessage = $"Unknown device type {dev.Type}.";
                    return 0;
            }
        }

        internal bool FindReadCommand()
        {
            switch(dev.Type)
            {
                case DeviceType.ATA: return AtaFindReadCommand();
                case DeviceType.ATAPI:
                case DeviceType.SCSI: return ScsiFindReadCommand();
                default:
                    ErrorMessage = $"Unknown device type {dev.Type}.";
                    return true;
            }
        }

        internal bool GetBlockSize()
        {
            switch(dev.Type)
            {
                case DeviceType.ATA: return AtaGetBlockSize();
                case DeviceType.ATAPI:
                case DeviceType.SCSI: return ScsiGetBlockSize();
                default:
                    ErrorMessage = $"Unknown device type {dev.Type}.";
                    return true;
            }
        }

        internal bool GetBlocksToRead(uint startWithBlocks = 64)
        {
            switch(dev.Type)
            {
                case DeviceType.ATA: return AtaGetBlocksToRead(startWithBlocks);
                case DeviceType.ATAPI:
                case DeviceType.SCSI: return ScsiGetBlocksToRead(startWithBlocks);
                default:
                    ErrorMessage = $"Unknown device type {dev.Type}.";
                    return true;
            }
        }

        internal bool ReadBlock(out byte[] buffer, ulong block, out double duration)
        {
            return ReadBlocks(out buffer, block, 1, out duration);
        }

        internal bool ReadBlocks(out byte[] buffer, ulong block, out double duration)
        {
            return ReadBlocks(out buffer, block, BlocksToRead, out duration);
        }

        internal bool ReadBlocks(out byte[] buffer, ulong block, uint count, out double duration)
        {
            switch(dev.Type)
            {
                case DeviceType.ATA: return AtaReadBlocks(out buffer, block, count, out duration);
                case DeviceType.ATAPI:
                case DeviceType.SCSI: return ScsiReadBlocks(out buffer, block, count, out duration);
                default:
                    buffer = null;
                    duration = 0d;
                    return true;
            }
        }

        internal bool ReadChs(out byte[] buffer, ushort cylinder, byte head, byte sector, out double duration)
        {
            switch(dev.Type)
            {
                case DeviceType.ATA: return AtaReadChs(out buffer, cylinder, head, sector, out duration);
                default:
                    buffer = null;
                    duration = 0d;
                    return true;
            }
        }

        internal bool Seek(ulong block, out double duration)
        {
            switch(dev.Type)
            {
                case DeviceType.ATA: return AtaSeek(block, out duration);
                case DeviceType.ATAPI:
                case DeviceType.SCSI: return ScsiSeek(block, out duration);
                default:
                    duration = 0d;
                    return true;
            }
        }

        internal bool SeekChs(ushort cylinder, byte head, byte sector, out double duration)
        {
            switch(dev.Type)
            {
                case DeviceType.ATA: return AtaSeekChs(cylinder, head, sector, out duration);
                default:
                    duration = 0;
                    return true;
            }
        }
    }
}