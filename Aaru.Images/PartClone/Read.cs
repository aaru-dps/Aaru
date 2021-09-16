// /***************************************************************************
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
//     Reads partclone disk images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class PartClone
    {
        /// <inheritdoc />
        public ErrorNumber Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return ErrorNumber.InvalidArgument;

            byte[] pHdrB = new byte[Marshal.SizeOf<Header>()];
            stream.Read(pHdrB, 0, Marshal.SizeOf<Header>());
            _pHdr = Marshal.ByteArrayToStructureLittleEndian<Header>(pHdrB);

            AaruConsole.DebugWriteLine("PartClone plugin", "pHdr.magic = {0}", StringHandlers.CToString(_pHdr.magic));

            AaruConsole.DebugWriteLine("PartClone plugin", "pHdr.filesystem = {0}",
                                       StringHandlers.CToString(_pHdr.filesystem));

            AaruConsole.DebugWriteLine("PartClone plugin", "pHdr.version = {0}",
                                       StringHandlers.CToString(_pHdr.version));

            AaruConsole.DebugWriteLine("PartClone plugin", "pHdr.blockSize = {0}", _pHdr.blockSize);
            AaruConsole.DebugWriteLine("PartClone plugin", "pHdr.deviceSize = {0}", _pHdr.deviceSize);
            AaruConsole.DebugWriteLine("PartClone plugin", "pHdr.totalBlocks = {0}", _pHdr.totalBlocks);
            AaruConsole.DebugWriteLine("PartClone plugin", "pHdr.usedBlocks = {0}", _pHdr.usedBlocks);

            _byteMap = new byte[_pHdr.totalBlocks];
            AaruConsole.DebugWriteLine("PartClone plugin", "Reading bytemap {0} bytes", _byteMap.Length);
            stream.Read(_byteMap, 0, _byteMap.Length);

            byte[] bitmagic = new byte[8];
            stream.Read(bitmagic, 0, 8);

            AaruConsole.DebugWriteLine("PartClone plugin", "pHdr.bitmagic = {0}", StringHandlers.CToString(bitmagic));

            if(!_biTmAgIc.SequenceEqual(bitmagic))
                throw new ImageNotSupportedException("Could not find partclone BiTmAgIc, not continuing...");

            _dataOff = stream.Position;
            AaruConsole.DebugWriteLine("PartClone plugin", "pHdr.dataOff = {0}", _dataOff);

            AaruConsole.DebugWriteLine("PartClone plugin", "Filling extents");
            DateTime start = DateTime.Now;
            _extents    = new ExtentsULong();
            _extentsOff = new Dictionary<ulong, ulong>();
            bool  current     = _byteMap[0] > 0;
            ulong blockOff    = 0;
            ulong extentStart = 0;

            for(ulong i = 1; i < _pHdr.totalBlocks; i++)
            {
                bool next = _byteMap[i] > 0;

                // Flux
                if(next != current)
                    if(next)
                    {
                        extentStart = i;
                        _extentsOff.Add(i, ++blockOff);
                    }
                    else
                    {
                        _extents.Add(extentStart, i);
                        _extentsOff.TryGetValue(extentStart, out _);
                    }

                if(next && current)
                    blockOff++;

                current = next;
            }

            DateTime end = DateTime.Now;

            AaruConsole.DebugWriteLine("PartClone plugin", "Took {0} seconds to fill extents",
                                       (end - start).TotalSeconds);

            _sectorCache = new Dictionary<ulong, byte[]>();

            _imageInfo.CreationTime         = imageFilter.CreationTime;
            _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
            _imageInfo.Sectors              = _pHdr.totalBlocks;
            _imageInfo.SectorSize           = _pHdr.blockSize;
            _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            _imageInfo.MediaType            = MediaType.GENERIC_HDD;
            _imageInfo.ImageSize            = (ulong)(stream.Length - (4096 + 0x40 + (long)_pHdr.totalBlocks));
            _imageStream                    = stream;

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(_byteMap[sectorAddress] == 0)
                return new byte[_pHdr.blockSize];

            if(_sectorCache.TryGetValue(sectorAddress, out byte[] sector))
                return sector;

            long imageOff = _dataOff + (long)(BlockOffset(sectorAddress) * (_pHdr.blockSize + CRC_SIZE));

            sector = new byte[_pHdr.blockSize];
            _imageStream.Seek(imageOff, SeekOrigin.Begin);
            _imageStream.Read(sector, 0, (int)_pHdr.blockSize);

            if(_sectorCache.Count > MAX_CACHED_SECTORS)
                _sectorCache.Clear();

            _sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var ms = new MemoryStream();

            bool allEmpty = true;

            for(uint i = 0; i < length; i++)
                if(_byteMap[sectorAddress + i] != 0)
                {
                    allEmpty = false;

                    break;
                }

            if(allEmpty)
                return new byte[_pHdr.blockSize * length];

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}