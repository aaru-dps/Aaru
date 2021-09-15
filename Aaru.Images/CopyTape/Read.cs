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
//     Reads CopyTape tape images.
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
using System.Text;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    public sealed partial class CopyTape
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            List<long> blockPositions = new();
            var        partialBlockRx = new Regex(PARTIAL_BLOCK_REGEX);
            var        blockRx        = new Regex(BLOCK_REGEX);
            var        filemarkRx     = new Regex(FILEMARK_REGEX);
            var        eotRx          = new Regex(END_OF_TAPE_REGEX);

            if(imageFilter.DataForkLength <= 16)
                return false;

            _imageStream          = imageFilter.GetDataForkStream();
            _imageStream.Position = 0;

            byte[] header           = new byte[9];
            byte[] blockHeader      = new byte[16];
            ulong  currentBlock     = 0;
            uint   currentFile      = 0;
            ulong  currentFileStart = 0;
            bool   inFile           = false;

            Files = new List<TapeFile>();

            while(_imageStream.Position + 9 < _imageStream.Length)
            {
                _imageStream.Read(header, 0, 9);
                string mark = Encoding.ASCII.GetString(header);

                Match partialBlockMt = partialBlockRx.Match(mark);
                Match filemarkMt     = filemarkRx.Match(mark);
                Match eotMt          = eotRx.Match(mark);

                if(eotMt.Success)
                    break;

                if(filemarkMt.Success)
                {
                    Files.Add(new TapeFile
                    {
                        File       = currentFile,
                        FirstBlock = currentFileStart,
                        LastBlock  = currentBlock - 1,
                        Partition  = 0
                    });

                    inFile = false;
                    currentFile++;

                    continue;
                }

                if(!partialBlockMt.Success)
                    throw new ArgumentException("Found unhandled header, cannot open.");

                _imageStream.Position -= 9;

                if(!inFile)
                {
                    currentFileStart = currentBlock;
                    inFile           = true;
                }

                _imageStream.Read(blockHeader, 0, 16);
                mark = Encoding.ASCII.GetString(blockHeader);
                Match blockMt = blockRx.Match(mark);

                if(!blockMt.Success)
                    throw new ArgumentException("Cannot decode block header, cannot open.");

                string blkSize = blockMt.Groups["blockSize"].Value;

                if(string.IsNullOrWhiteSpace(blkSize))
                    throw new ArgumentException("Cannot decode block header, cannot open.");

                if(!uint.TryParse(blkSize, out uint blockSize))
                    throw new ArgumentException("Cannot decode block header, cannot open.");

                if(blockSize      == 0 ||
                   blockSize + 17 > imageFilter.DataForkLength)
                    throw new ArgumentException("Cannot decode block header, cannot open.");

                _imageStream.Position += blockSize;

                int newLine = _imageStream.ReadByte();

                if(newLine != 0x0A)
                    throw new ArgumentException("Cannot decode block header, cannot open.");

                blockPositions.Add(_imageStream.Position - blockSize - 17);
                currentBlock++;
                _imageInfo.ImageSize += blockSize;

                if(_imageInfo.SectorSize < blockSize)
                    _imageInfo.SectorSize = blockSize;
            }

            _blockPositionCache = blockPositions.ToArray();

            TapePartitions = new List<TapePartition>
            {
                new()
                {
                    FirstBlock = 0,
                    LastBlock  = currentBlock - 1,
                    Number     = 0
                }
            };

            _imageInfo.Sectors              = (ulong)_blockPositionCache.LongLength;
            _imageInfo.MediaType            = MediaType.UnknownTape;
            _imageInfo.Application          = "CopyTape";
            _imageInfo.CreationTime         = imageFilter.CreationTime;
            _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
            _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            IsTape                          = true;

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress >= (ulong)_blockPositionCache.LongLength)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            _imageStream.Position = _blockPositionCache[sectorAddress];

            byte[] blockHeader = new byte[16];
            var    blockRx     = new Regex(BLOCK_REGEX);

            _imageStream.Read(blockHeader, 0, 16);
            string mark    = Encoding.ASCII.GetString(blockHeader);
            Match  blockMt = blockRx.Match(mark);

            if(!blockMt.Success)
                throw new ArgumentException("Cannot decode block header, cannot read.");

            string blkSize = blockMt.Groups["blockSize"].Value;

            if(string.IsNullOrWhiteSpace(blkSize))
                throw new ArgumentException("Cannot decode block header, cannot read.");

            if(!uint.TryParse(blkSize, out uint blockSize))
                throw new ArgumentException("Cannot decode block header, cannot read.");

            if(blockSize      == 0 ||
               blockSize + 17 > _imageStream.Length)
                throw new ArgumentException("Cannot decode block header, cannot read.");

            byte[] data = new byte[blockSize];

            _imageStream.Read(data, 0, (int)blockSize);

            if(_imageStream.ReadByte() != 0x0A)
                throw new ArgumentException("Cannot decode block header, cannot read.");

            return data;
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            var dataMs = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] data = ReadSector(sectorAddress + i);
                dataMs.Write(data, 0, data.Length);
            }

            return dataMs.ToArray();
        }
    }
}