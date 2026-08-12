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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Images;

public sealed partial class CopyTape
{
#region IWritableTapeImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        List<long> blockPositions = [];
        Regex      partialBlockRx = PartialBlockRegex();
        Regex      blockRx        = BlockRegex();
        Regex      filemarkRx     = FilemarkRegex();
        Regex      eotRx          = EndOfTapeRegex();

        if(imageFilter.DataForkLength <= 16) return ErrorNumber.InvalidArgument;

        _imageStream          = imageFilter.GetDataForkStream();
        _imageStream.Position = 0;

        var   header           = new byte[9];
        var   blockHeader      = new byte[16];
        ulong currentBlock     = 0;
        uint  currentFile      = 0;
        ulong currentFileStart = 0;
        var   inFile           = false;

        Files = [];

        while(_imageStream.Position + 9 < _imageStream.Length)
        {
            _imageStream.EnsureRead(header, 0, 9);
            string mark = Encoding.ASCII.GetString(header);

            Match partialBlockMt = partialBlockRx.Match(mark);
            Match filemarkMt     = filemarkRx.Match(mark);
            Match eotMt          = eotRx.Match(mark);

            if(eotMt.Success) break;

            if(filemarkMt.Success)
            {
                // A filemark with no preceding block would underflow LastBlock
                if(inFile)
                {
                    Files.Add(new TapeFile
                    {
                        File       = currentFile,
                        FirstBlock = currentFileStart,
                        LastBlock  = currentBlock - 1,
                        Partition  = 0
                    });
                }

                inFile = false;
                currentFile++;

                continue;
            }

            if(!partialBlockMt.Success)
            {
                AaruLogging.Error(Localization.Found_unhandled_header_cannot_open);

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
                AaruLogging.Error(Localization.Cannot_decode_block_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

            string blkSize = blockMt.Groups["blockSize"].Value;

            if(string.IsNullOrWhiteSpace(blkSize))
            {
                AaruLogging.Error(Localization.Cannot_decode_block_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

            if(!uint.TryParse(blkSize, out uint blockSize))
            {
                AaruLogging.Error(Localization.Cannot_decode_block_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

            if(blockSize == 0 || (long)blockSize + 17 > imageFilter.DataForkLength)
            {
                AaruLogging.Error(Localization.Cannot_decode_block_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

            _imageStream.Position += blockSize;

            int newLine = _imageStream.ReadByte();

            if(newLine != 0x0A)
            {
                AaruLogging.Error(Localization.Cannot_decode_block_header_cannot_open);

                return ErrorNumber.InvalidArgument;
            }

            blockPositions.Add(_imageStream.Position - blockSize - 17);
            currentBlock++;
            _imageInfo.ImageSize += blockSize;

            if(_imageInfo.SectorSize < blockSize) _imageInfo.SectorSize = blockSize;
        }

        // Close the last file when the tape lacks a trailing filemark
        if(inFile)
        {
            Files.Add(new TapeFile
            {
                File       = currentFile,
                FirstBlock = currentFileStart,
                LastBlock  = currentBlock - 1,
                Partition  = 0
            });
        }

        _blockPositionCache = blockPositions.ToArray();

        TapePartitions =
        [
            new TapePartition
            {
                FirstBlock = 0,
                LastBlock  = currentBlock - 1,
                Number     = 0
            }
        ];

        _imageInfo.Sectors              = (ulong)_blockPositionCache.LongLength;
        _imageInfo.MediaType            = MediaType.UnknownTape;
        _imageInfo.Application          = "CopyTape";
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        IsTape                          = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress >= (ulong)_blockPositionCache.LongLength) return ErrorNumber.OutOfRange;

        _imageStream.Position = _blockPositionCache[sectorAddress];

        var blockHeader = new byte[16];
        var blockRx     = new Regex(BLOCK_REGEX);

        _imageStream.EnsureRead(blockHeader, 0, 16);
        string mark    = Encoding.ASCII.GetString(blockHeader);
        Match  blockMt = blockRx.Match(mark);

        if(!blockMt.Success) return ErrorNumber.InvalidArgument;

        string blkSize = blockMt.Groups["blockSize"].Value;

        if(string.IsNullOrWhiteSpace(blkSize)) return ErrorNumber.InvalidArgument;

        if(!uint.TryParse(blkSize, out uint blockSize)) return ErrorNumber.InvalidArgument;

        if(blockSize == 0 || (long)blockSize + 17 > _imageStream.Length) return ErrorNumber.InvalidArgument;

        buffer = new byte[blockSize];

        _imageStream.EnsureRead(buffer, 0, (int)blockSize);

        if(_imageStream.ReadByte() != 0x0A) return ErrorNumber.InvalidArgument;

        sectorStatus = SectorStatus.Dumped;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        var ms = new MemoryStream();
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, false, out byte[] sector, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sector, 0, sector.Length);
            sectorStatus[i] = status;
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion
}