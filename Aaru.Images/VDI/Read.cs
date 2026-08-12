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
//     Reads VirtualBox disk images.
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Images;

public sealed partial class Vdi
{
#region IWritableImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512) return ErrorNumber.InvalidArgument;

        var vHdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(vHdrB, 0, Marshal.SizeOf<Header>());
        _vHdr = Marshal.ByteArrayToStructureLittleEndian<Header>(vHdrB);

        AaruLogging.Debug(MODULE_NAME, "vHdr.creator = {0}", _vHdr.creator);
        AaruLogging.Debug(MODULE_NAME, "vHdr.magic = {0}",   _vHdr.magic);

        AaruLogging.Debug(MODULE_NAME, "vHdr.version = {0}.{1}", _vHdr.majorVersion, _vHdr.minorVersion);

        AaruLogging.Debug(MODULE_NAME, "vHdr.headerSize = {0}",        _vHdr.headerSize);
        AaruLogging.Debug(MODULE_NAME, "vHdr.imageType = {0}",         _vHdr.imageType);
        AaruLogging.Debug(MODULE_NAME, "vHdr.imageFlags = {0}",        _vHdr.imageFlags);
        AaruLogging.Debug(MODULE_NAME, "vHdr.description = {0}",       _vHdr.comments);
        AaruLogging.Debug(MODULE_NAME, "vHdr.offsetBlocks = {0}",      _vHdr.offsetBlocks);
        AaruLogging.Debug(MODULE_NAME, "vHdr.offsetData = {0}",        _vHdr.offsetData);
        AaruLogging.Debug(MODULE_NAME, "vHdr.cylinders = {0}",         _vHdr.cylinders);
        AaruLogging.Debug(MODULE_NAME, "vHdr.heads = {0}",             _vHdr.heads);
        AaruLogging.Debug(MODULE_NAME, "vHdr.spt = {0}",               _vHdr.spt);
        AaruLogging.Debug(MODULE_NAME, "vHdr.sectorSize = {0}",        _vHdr.sectorSize);
        AaruLogging.Debug(MODULE_NAME, "vHdr.size = {0}",              _vHdr.size);
        AaruLogging.Debug(MODULE_NAME, "vHdr.blockSize = {0}",         _vHdr.blockSize);
        AaruLogging.Debug(MODULE_NAME, "vHdr.blockExtraData = {0}",    _vHdr.blockExtraData);
        AaruLogging.Debug(MODULE_NAME, "vHdr.blocks = {0}",            _vHdr.blocks);
        AaruLogging.Debug(MODULE_NAME, "vHdr.allocatedBlocks = {0}",   _vHdr.allocatedBlocks);
        AaruLogging.Debug(MODULE_NAME, "vHdr.uuid = {0}",              _vHdr.uuid);
        AaruLogging.Debug(MODULE_NAME, "vHdr.snapshotUuid = {0}",      _vHdr.snapshotUuid);
        AaruLogging.Debug(MODULE_NAME, "vHdr.linkUuid = {0}",          _vHdr.linkUuid);
        AaruLogging.Debug(MODULE_NAME, "vHdr.parentUuid = {0}",        _vHdr.parentUuid);
        AaruLogging.Debug(MODULE_NAME, "vHdr.logicalCylinders = {0}",  _vHdr.logicalCylinders);
        AaruLogging.Debug(MODULE_NAME, "vHdr.logicalHeads = {0}",      _vHdr.logicalHeads);
        AaruLogging.Debug(MODULE_NAME, "vHdr.logicalSpt = {0}",        _vHdr.logicalSpt);
        AaruLogging.Debug(MODULE_NAME, "vHdr.logicalSectorSize = {0}", _vHdr.logicalSectorSize);

        if(_vHdr.imageType != VdiImageType.Normal)
        {
            AaruLogging.Error(string.Format(Localization.Support_for_image_type_0_not_yet_implemented,
                                            _vHdr.imageType));

            return ErrorNumber.InvalidArgument;
        }

        var blockMapStopwatch = new Stopwatch();
        blockMapStopwatch.Start();
        AaruLogging.Debug(MODULE_NAME, Localization.Reading_Image_Block_Map);
        stream.Seek(_vHdr.offsetBlocks, SeekOrigin.Begin);
        var ibmB = new byte[_vHdr.blocks * 4];
        stream.EnsureRead(ibmB, 0, ibmB.Length);
        _ibm = MemoryMarshal.Cast<byte, uint>(ibmB).ToArray();
        blockMapStopwatch.Stop();

        AaruLogging.Debug(MODULE_NAME,
                          Localization.Reading_Image_Block_Map_took_0_ms,
                          blockMapStopwatch.Elapsed.TotalMilliseconds);

        _sectorCache = new Dictionary<ulong, byte[]>();

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = _vHdr.size / _vHdr.sectorSize;
        _imageInfo.ImageSize            = _vHdr.size;
        _imageInfo.SectorSize           = _vHdr.sectorSize;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.MediaType            = MediaType.GENERIC_HDD;
        _imageInfo.Comments             = _vHdr.comments;
        _imageInfo.Version              = $"{_vHdr.majorVersion}.{_vHdr.minorVersion}";

        _imageInfo.Application = _vHdr.creator switch
                                 {
                                     SUN_VDI                        => "Sun VirtualBox",
                                     SUN_OLD_VDI                    => "Sun xVM",
                                     ORACLE_VDI                     => "Oracle VirtualBox",
                                     QEMUVDI                        => "QEMU",
                                     INNOTEK_VDI or INNOTEK_OLD_VDI => "innotek VirtualBox",
                                     DIC_VDI                        => "DiscImageChef",
                                     DIC_AARU                       => "Aaru",
                                     _                              => _imageInfo.Application
                                 };

        _imageStream = stream;

        if(_vHdr.headerSize >= 400)
        {
            _imageInfo.Cylinders       = _vHdr.logicalCylinders;
            _imageInfo.Heads           = _vHdr.logicalHeads;
            _imageInfo.SectorsPerTrack = _vHdr.logicalSpt;
        }
        else
        {
            _imageInfo.Cylinders       = _vHdr.cylinders;
            _imageInfo.Heads           = _vHdr.heads;
            _imageInfo.SectorsPerTrack = _vHdr.spt;
        }

        if(_imageInfo.Cylinders != 0) return ErrorNumber.NoError;

        // Same calculation as done by VirtualBox
        _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
        _imageInfo.Heads           = 16;
        _imageInfo.SectorsPerTrack = 63;

        while(_imageInfo.Cylinders == 0)
        {
            _imageInfo.Heads--;

            if(_imageInfo.Heads == 0)
            {
                _imageInfo.SectorsPerTrack--;

                if(_imageInfo.SectorsPerTrack == 0) break;

                _imageInfo.Heads = 16;
            }

            _imageInfo.Cylinders = (uint)(_imageInfo.Sectors / _imageInfo.Heads / _imageInfo.SectorsPerTrack);
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(_sectorCache.TryGetValue(sectorAddress, out buffer)) return ErrorNumber.NoError;

        sectorStatus = SectorStatus.Dumped;

        ulong index  = sectorAddress * _vHdr.sectorSize / _vHdr.blockSize;
        ulong secOff = sectorAddress * _vHdr.sectorSize % _vHdr.blockSize;

        if(index >= (ulong)_ibm.LongLength) return ErrorNumber.OutOfRange;

        uint ibmOff = _ibm[(int)index];

        // VDI_EMPTY (unallocated) or VDI_IMAGE_BLOCK_ZERO (allocated, reads as zeros)
        if(ibmOff >= 0xFFFFFFFE)
        {
            buffer = new byte[_vHdr.sectorSize];

            return ErrorNumber.NoError;
        }

        ulong imageOff = _vHdr.offsetData + (ulong)ibmOff * _vHdr.blockSize;

        var cluster = new byte[_vHdr.blockSize];
        _imageStream.Seek((long)imageOff, SeekOrigin.Begin);
        _imageStream.EnsureRead(cluster, 0, (int)_vHdr.blockSize);
        buffer = new byte[_vHdr.sectorSize];
        Array.Copy(cluster, (int)secOff, buffer, 0, _vHdr.sectorSize);

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