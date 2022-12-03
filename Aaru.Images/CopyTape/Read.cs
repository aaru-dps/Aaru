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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class CopyTape
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        List<long> blockPositions = new();
        var        partialBlockRx = new Regex(PARTIAL_BLOCK_REGEX);
        var        blockRx        = new Regex(BLOCK_REGEX);
        var        filemarkRx     = new Regex(FILEMARK_REGEX);
        var        eotRx          = new Regex(END_OF_TAPE_REGEX);

        if(imageFilter.DataForkLength <= 16)
            return ErrorNumber.InvalidArgument;

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
            _imageStream.EnsureRead(header, 0, 9);
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
            {
                AaruConsole.ErrorWriteLine(Localization.Found_unhandled_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

            _imageStream.Position -= 9;

            if(!inFile)
            {
                currentFileStart = currentBlock;
                inFile           = true;
            }

            _imageStream.EnsureRead(blockHeader, 0, 16);
            mark = Encoding.ASCII.GetString(blockHeader);
            Match blockMt = blockRx.Match(mark);

            if(!blockMt.Success)
            {
                AaruConsole.ErrorWriteLine(Localization.Cannot_decode_block_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

            string blkSize = blockMt.Groups["blockSize"].Value;

            if(string.IsNullOrWhiteSpace(blkSize))
            {
                AaruConsole.ErrorWriteLine(Localization.Cannot_decode_block_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

            if(!uint.TryParse(blkSize, out uint blockSize))
            {
                AaruConsole.ErrorWriteLine(Localization.Cannot_decode_block_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

            if(blockSize      == 0 ||
               blockSize + 17 > imageFilter.DataForkLength)
            {
                AaruConsole.ErrorWriteLine(Localization.Cannot_decode_block_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

            _imageStream.Position += blockSize;

            int newLine = _imageStream.ReadByte();

            if(newLine != 0x0A)
            {
                AaruConsole.ErrorWriteLine(Localization.Cannot_decode_block_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

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

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress >= (ulong)_blockPositionCache.LongLength)
            return ErrorNumber.OutOfRange;

        _imageStream.Position = _blockPositionCache[sectorAddress];

        byte[] blockHeader = new byte[16];
        var    blockRx     = new Regex(BLOCK_REGEX);

        _imageStream.EnsureRead(blockHeader, 0, 16);
        string mark    = Encoding.ASCII.GetString(blockHeader);
        Match  blockMt = blockRx.Match(mark);

        if(!blockMt.Success)
            return ErrorNumber.InvalidArgument;

        string blkSize = blockMt.Groups["blockSize"].Value;

        if(string.IsNullOrWhiteSpace(blkSize))
            return ErrorNumber.InvalidArgument;

        if(!uint.TryParse(blkSize, out uint blockSize))
            return ErrorNumber.InvalidArgument;

        if(blockSize      == 0 ||
           blockSize + 17 > _imageStream.Length)
            return ErrorNumber.InvalidArgument;

        buffer = new byte[blockSize];

        _imageStream.EnsureRead(buffer, 0, (int)blockSize);

        return _imageStream.ReadByte() != 0x0A ? ErrorNumber.InvalidArgument : ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }
}