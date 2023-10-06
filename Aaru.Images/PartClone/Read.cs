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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class PartClone
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512)
            return ErrorNumber.InvalidArgument;

        var pHdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(pHdrB, 0, Marshal.SizeOf<Header>());
        _pHdr = Marshal.ByteArrayToStructureLittleEndian<Header>(pHdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.magic = {0}", StringHandlers.CToString(_pHdr.magic));

        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.filesystem = {0}", StringHandlers.CToString(_pHdr.filesystem));

        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.version = {0}", StringHandlers.CToString(_pHdr.version));

        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.blockSize = {0}",   _pHdr.blockSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.deviceSize = {0}",  _pHdr.deviceSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.totalBlocks = {0}", _pHdr.totalBlocks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.usedBlocks = {0}",  _pHdr.usedBlocks);

        _byteMap = new byte[_pHdr.totalBlocks];
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_bytemap_0_bytes, _byteMap.Length);
        stream.EnsureRead(_byteMap, 0, _byteMap.Length);

        var bitmagic = new byte[8];
        stream.EnsureRead(bitmagic, 0, 8);

        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.bitmagic = {0}", StringHandlers.CToString(bitmagic));

        if(!_biTmAgIc.SequenceEqual(bitmagic))
        {
            AaruConsole.ErrorWriteLine(Localization.Could_not_find_partclone_BiTmAgIc_not_continuing);

            return ErrorNumber.InvalidArgument;
        }

        _dataOff = stream.Position;
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.dataOff = {0}", _dataOff);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Filling_extents);
        var extentFillStopwatch = new Stopwatch();
        extentFillStopwatch.Start();
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
            {
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
            }

            if(next && current)
                blockOff++;

            current = next;
        }

        extentFillStopwatch.Stop();

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Took_0_seconds_to_fill_extents,
                                   extentFillStopwatch.Elapsed.TotalSeconds);

        _sectorCache = new Dictionary<ulong, byte[]>();

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = _pHdr.totalBlocks;
        _imageInfo.SectorSize           = _pHdr.blockSize;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.MediaType            = MediaType.GENERIC_HDD;
        _imageInfo.ImageSize            = (ulong)(stream.Length - (4096 + 0x40 + (long)_pHdr.totalBlocks));
        _imageStream                    = stream;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(_byteMap[sectorAddress] == 0)
        {
            buffer = new byte[_pHdr.blockSize];

            return ErrorNumber.NoError;
        }

        if(_sectorCache.TryGetValue(sectorAddress, out buffer))
            return ErrorNumber.NoError;

        long imageOff = _dataOff + (long)(BlockOffset(sectorAddress) * (_pHdr.blockSize + CRC_SIZE));

        buffer = new byte[_pHdr.blockSize];
        _imageStream.Seek(imageOff, SeekOrigin.Begin);
        _imageStream.EnsureRead(buffer, 0, (int)_pHdr.blockSize);

        if(_sectorCache.Count > MAX_CACHED_SECTORS)
            _sectorCache.Clear();

        _sectorCache.Add(sectorAddress, buffer);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        var allEmpty = true;

        for(uint i = 0; i < length; i++)
        {
            if(_byteMap[sectorAddress + i] == 0)
                continue;

            allEmpty = false;

            break;
        }

        if(allEmpty)
        {
            buffer = new byte[_pHdr.blockSize * length];

            return ErrorNumber.NoError;
        }

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

#endregion
}