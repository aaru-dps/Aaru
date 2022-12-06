﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads DiscFerret flux images.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class DiscFerret
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            byte[] magicB = new byte[4];
            Stream stream = imageFilter.GetDataForkStream();
            stream.Read(magicB, 0, 4);
            uint magic = BitConverter.ToUInt32(magicB, 0);

            if(magic != DFI_MAGIC &&
               magic != DFI_MAGIC2)
                return false;

            TrackOffsets = new SortedDictionary<int, long>();
            TrackLengths = new SortedDictionary<int, long>();
            int    t            = -1;
            ushort lastCylinder = 0, lastHead = 0;
            long   offset       = 0;

            while(stream.Position < stream.Length)
            {
                long thisOffset = stream.Position;

                byte[] blk = new byte[Marshal.SizeOf<BlockHeader>()];
                stream.Read(blk, 0, Marshal.SizeOf<BlockHeader>());
                BlockHeader blockHeader = Marshal.ByteArrayToStructureBigEndian<BlockHeader>(blk);

                AaruConsole.DebugWriteLine("DiscFerret plugin", "block@{0}.cylinder = {1}", thisOffset,
                                           blockHeader.cylinder);

                AaruConsole.DebugWriteLine("DiscFerret plugin", "block@{0}.head = {1}", thisOffset, blockHeader.head);

                AaruConsole.DebugWriteLine("DiscFerret plugin", "block@{0}.sector = {1}", thisOffset,
                                           blockHeader.sector);

                AaruConsole.DebugWriteLine("DiscFerret plugin", "block@{0}.length = {1}", thisOffset,
                                           blockHeader.length);

                if(stream.Position + blockHeader.length > stream.Length)
                {
                    AaruConsole.DebugWriteLine("DiscFerret plugin", "Invalid track block found at {0}", thisOffset);

                    break;
                }

                stream.Position += blockHeader.length;

                if(blockHeader.cylinder > 0 &&
                   blockHeader.cylinder > lastCylinder)
                {
                    lastCylinder = blockHeader.cylinder;
                    lastHead     = 0;
                    TrackOffsets.Add(t, offset);
                    TrackLengths.Add(t, thisOffset - offset + 1);
                    offset = thisOffset;
                    t++;
                }
                else if(blockHeader.head > 0 &&
                        blockHeader.head > lastHead)
                {
                    lastHead = blockHeader.head;
                    TrackOffsets.Add(t, offset);
                    TrackLengths.Add(t, thisOffset - offset + 1);
                    offset = thisOffset;
                    t++;
                }

                if(blockHeader.cylinder > _imageInfo.Cylinders)
                    _imageInfo.Cylinders = blockHeader.cylinder;

                if(blockHeader.head > _imageInfo.Heads)
                    _imageInfo.Heads = blockHeader.head;
            }

            _imageInfo.Heads++;
            _imageInfo.Cylinders++;

            _imageInfo.Application        = "DiscFerret";
            _imageInfo.ApplicationVersion = magic == DFI_MAGIC2 ? "2.0" : "1.0";

            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");
    }
}