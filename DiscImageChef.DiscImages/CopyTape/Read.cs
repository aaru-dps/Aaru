// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.DiscImages.CopyTape
{
    public partial class CopyTape
    {
        public bool Open(IFilter imageFilter)
        {
            List<long> blockPositions = new List<long>();
            Regex      partialBlockRx = new Regex(PartialBlockRegex);
            Regex      blockRx        = new Regex(BlockRegex);
            Regex      filemarkRx     = new Regex(FilemarkRegex);
            Regex      eotRx          = new Regex(EndOfTapeRegex);

            if(imageFilter.GetDataForkLength() <= 16) return false;

            imageStream          = imageFilter.GetDataForkStream();
            imageStream.Position = 0;

            byte[] header           = new byte[9];
            byte[] blockHeader      = new byte[16];
            ulong  currentBlock     = 0;
            uint   currentFile      = 0;
            ulong  currentFileStart = 0;
            bool   inFile           = false;

            Files = new List<TapeFile>();

            while(imageStream.Position + 9 < imageStream.Length)
            {
                imageStream.Read(header, 0, 9);
                string mark = Encoding.ASCII.GetString(header);

                Match partialBlockMt = partialBlockRx.Match(mark);
                Match filemarkMt     = filemarkRx.Match(mark);
                Match eotMt          = eotRx.Match(mark);

                if(eotMt.Success) break;

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

                if(!partialBlockMt.Success) throw new ArgumentException("Found unhandled header, cannot open.");

                imageStream.Position -= 9;

                if(!inFile)
                {
                    currentFileStart = currentBlock;
                    inFile           = true;
                }

                imageStream.Read(blockHeader, 0, 16);
                mark = Encoding.ASCII.GetString(blockHeader);
                Match blockMt = blockRx.Match(mark);

                if(!blockMt.Success) throw new ArgumentException("Cannot decode block header, cannot open.");

                string blkSize = blockMt.Groups["blockSize"].Value;

                if(string.IsNullOrWhiteSpace(blkSize))
                    throw new ArgumentException("Cannot decode block header, cannot open.");

                if(!uint.TryParse(blkSize, out uint blockSize))
                    throw new ArgumentException("Cannot decode block header, cannot open.");

                if(blockSize == 0 || blockSize + 17 > imageFilter.GetDataForkLength())
                    throw new ArgumentException("Cannot decode block header, cannot open.");

                imageStream.Position += blockSize;

                int newLine = imageStream.ReadByte();

                if(newLine != 0x0A) throw new ArgumentException("Cannot decode block header, cannot open.");

                blockPositions.Add(imageStream.Position - blockSize - 17);
                currentBlock++;
                imageInfo.ImageSize += blockSize;
                if(imageInfo.SectorSize < blockSize) imageInfo.SectorSize = blockSize;
            }

            blockPositionCache = blockPositions.ToArray();

            TapePartitions = new List<TapePartition>
            {
                new TapePartition {FirstBlock = 0, LastBlock = currentBlock - 1, Number = 0}
            };

            imageInfo.Sectors              = (ulong)blockPositionCache.LongLength;
            imageInfo.MediaType            = MediaType.UnknownTape;
            imageInfo.Application          = "CopyTape";
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress >= (ulong)blockPositionCache.LongLength)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            imageStream.Position = blockPositionCache[sectorAddress];

            byte[] blockHeader = new byte[16];
            Regex  blockRx     = new Regex(BlockRegex);

            imageStream.Read(blockHeader, 0, 16);
            string mark    = Encoding.ASCII.GetString(blockHeader);
            Match  blockMt = blockRx.Match(mark);

            if(!blockMt.Success) throw new ArgumentException("Cannot decode block header, cannot read.");

            string blkSize = blockMt.Groups["blockSize"].Value;

            if(string.IsNullOrWhiteSpace(blkSize))
                throw new ArgumentException("Cannot decode block header, cannot read.");

            if(!uint.TryParse(blkSize, out uint blockSize))
                throw new ArgumentException("Cannot decode block header, cannot read.");

            if(blockSize == 0 || blockSize + 17 > imageStream.Length)
                throw new ArgumentException("Cannot decode block header, cannot read.");

            byte[] data = new byte[blockSize];

            imageStream.Read(data, 0, (int)blockSize);

            if(imageStream.ReadByte() != 0x0A) throw new ArgumentException("Cannot decode block header, cannot read.");

            return data;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            MemoryStream dataMs = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] data = ReadSector(sectorAddress + i);
                dataMs.Write(data, 0, data.Length);
            }

            return dataMs.ToArray();
        }
    }
}