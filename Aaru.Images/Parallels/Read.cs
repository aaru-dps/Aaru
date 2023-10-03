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
//     Reads Parallels disk images.
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
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class Parallels
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512)
            return ErrorNumber.InvalidArgument;

        byte[] pHdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(pHdrB, 0, Marshal.SizeOf<Header>());
        _pHdr = Marshal.ByteArrayToStructureLittleEndian<Header>(pHdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.magic = {0}", StringHandlers.CToString(_pHdr.magic));
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.version = {0}", _pHdr.version);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.heads = {0}", _pHdr.heads);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.cylinders = {0}", _pHdr.cylinders);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.cluster_size = {0}", _pHdr.cluster_size);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.bat_entries = {0}", _pHdr.bat_entries);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.sectors = {0}", _pHdr.sectors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.in_use = 0x{0:X8}", _pHdr.in_use);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.data_off = {0}", _pHdr.data_off);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.flags = {0}", _pHdr.flags);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.ext_off = {0}", _pHdr.ext_off);

        _extended = _extMagic.SequenceEqual(_pHdr.magic);
        AaruConsole.DebugWriteLine(MODULE_NAME, "pHdr.extended = {0}", _extended);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_BAT);
        _bat = new uint[_pHdr.bat_entries];
        byte[] batB = new byte[_pHdr.bat_entries * 4];
        stream.EnsureRead(batB, 0, batB.Length);

        for(int i = 0; i < _bat.Length; i++)
            _bat[i] = BitConverter.ToUInt32(batB, i * 4);

        _clusterBytes = _pHdr.cluster_size * 512;

        if(_pHdr.data_off > 0)
            _dataOffset = _pHdr.data_off * 512;
        else
            _dataOffset = ((stream.Position / _clusterBytes) + (stream.Position % _clusterBytes)) * _clusterBytes;

        _sectorCache = new Dictionary<ulong, byte[]>();

        _empty = (_pHdr.flags & PARALLELS_EMPTY) == PARALLELS_EMPTY;

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = _pHdr.sectors;
        _imageInfo.SectorSize           = 512;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.MediaType            = MediaType.GENERIC_HDD;
        _imageInfo.ImageSize            = _pHdr.sectors * 512;
        _imageInfo.Cylinders            = _pHdr.cylinders;
        _imageInfo.Heads                = _pHdr.heads;
        _imageInfo.SectorsPerTrack      = (uint)(_imageInfo.Sectors / _imageInfo.Cylinders / _imageInfo.Heads);
        _imageStream                    = stream;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(_empty)
        {
            buffer = new byte[512];

            return ErrorNumber.NoError;
        }

        if(_sectorCache.TryGetValue(sectorAddress, out buffer))
            return ErrorNumber.NoError;

        ulong index  = sectorAddress / _pHdr.cluster_size;
        ulong secOff = sectorAddress % _pHdr.cluster_size;

        uint  batOff = _bat[index];
        ulong imageOff;

        if(batOff == 0)
        {
            buffer = new byte[512];

            return ErrorNumber.NoError;
        }

        if(_extended)
            imageOff = (ulong)batOff * _clusterBytes;
        else
            imageOff = batOff * 512UL;

        byte[] cluster = new byte[_clusterBytes];
        _imageStream.Seek((long)imageOff, SeekOrigin.Begin);
        _imageStream.EnsureRead(cluster, 0, (int)_clusterBytes);
        buffer = new byte[512];
        Array.Copy(cluster, (int)(secOff * 512), buffer, 0, 512);

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