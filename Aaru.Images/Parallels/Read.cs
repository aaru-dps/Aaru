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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Images;

public sealed partial class Parallels
{
#region IWritableImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512) return ErrorNumber.InvalidArgument;

        var pHdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(pHdrB, 0, Marshal.SizeOf<Header>());
        _pHdr = Marshal.ByteArrayToStructureLittleEndian<Header>(pHdrB);

        AaruLogging.Debug(MODULE_NAME, "pHdr.magic = {0}",        StringHandlers.CToString(_pHdr.magic));
        AaruLogging.Debug(MODULE_NAME, "pHdr.version = {0}",      _pHdr.version);
        AaruLogging.Debug(MODULE_NAME, "pHdr.heads = {0}",        _pHdr.heads);
        AaruLogging.Debug(MODULE_NAME, "pHdr.cylinders = {0}",    _pHdr.cylinders);
        AaruLogging.Debug(MODULE_NAME, "pHdr.cluster_size = {0}", _pHdr.cluster_size);
        AaruLogging.Debug(MODULE_NAME, "pHdr.bat_entries = {0}",  _pHdr.bat_entries);
        AaruLogging.Debug(MODULE_NAME, "pHdr.sectors = {0}",      _pHdr.sectors);
        AaruLogging.Debug(MODULE_NAME, "pHdr.in_use = 0x{0:X8}",  _pHdr.in_use);
        AaruLogging.Debug(MODULE_NAME, "pHdr.data_off = {0}",     _pHdr.data_off);
        AaruLogging.Debug(MODULE_NAME, "pHdr.flags = {0}",        _pHdr.flags);
        AaruLogging.Debug(MODULE_NAME, "pHdr.ext_off = {0}",      _pHdr.ext_off);

        _extended = _extMagic.SequenceEqual(_pHdr.magic);
        AaruLogging.Debug(MODULE_NAME, "pHdr.extended = {0}", _extended);

        AaruLogging.Debug(MODULE_NAME, Localization.Reading_BAT);
        _bat = new uint[_pHdr.bat_entries];
        var batB = new byte[_pHdr.bat_entries * 4];
        stream.EnsureRead(batB, 0, batB.Length);

        for(var i = 0; i < _bat.Length; i++) _bat[i] = BitConverter.ToUInt32(batB, i * 4);

        _clusterBytes = _pHdr.cluster_size * 512;

        if(_pHdr.data_off > 0)
            _dataOffset = _pHdr.data_off * 512;
        else
            _dataOffset = (stream.Position + _clusterBytes - 1) / _clusterBytes * _clusterBytes;

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
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        sectorStatus = SectorStatus.Dumped;

        if(_empty)
        {
            buffer = new byte[512];

            return ErrorNumber.NoError;
        }

        if(_sectorCache.TryGetValue(sectorAddress, out buffer)) return ErrorNumber.NoError;

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

        var cluster = new byte[_clusterBytes];
        _imageStream.Seek((long)imageOff, SeekOrigin.Begin);
        _imageStream.EnsureRead(cluster, 0, (int)_clusterBytes);
        buffer = new byte[512];
        Array.Copy(cluster, (int)(secOff * 512), buffer, 0, 512);

        if(_sectorCache.Count > MAX_CACHED_SECTORS) _sectorCache.Clear();

        _sectorCache.Add(sectorAddress, buffer);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

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