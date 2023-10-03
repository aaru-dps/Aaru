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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages;

public sealed partial class Vdi
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512)
            return ErrorNumber.InvalidArgument;

        byte[] vHdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(vHdrB, 0, Marshal.SizeOf<Header>());
        _vHdr = Marshal.ByteArrayToStructureLittleEndian<Header>(vHdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.creator = {0}", _vHdr.creator);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.magic = {0}", _vHdr.magic);

        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.version = {0}.{1}", _vHdr.majorVersion,
                                   _vHdr.minorVersion);

        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.headerSize = {0}", _vHdr.headerSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.imageType = {0}", _vHdr.imageType);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.imageFlags = {0}", _vHdr.imageFlags);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.description = {0}", _vHdr.comments);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.offsetBlocks = {0}", _vHdr.offsetBlocks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.offsetData = {0}", _vHdr.offsetData);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.cylinders = {0}", _vHdr.cylinders);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.heads = {0}", _vHdr.heads);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.spt = {0}", _vHdr.spt);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.sectorSize = {0}", _vHdr.sectorSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.size = {0}", _vHdr.size);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.blockSize = {0}", _vHdr.blockSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.blockExtraData = {0}", _vHdr.blockExtraData);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.blocks = {0}", _vHdr.blocks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.allocatedBlocks = {0}", _vHdr.allocatedBlocks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.uuid = {0}", _vHdr.uuid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.snapshotUuid = {0}", _vHdr.snapshotUuid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.linkUuid = {0}", _vHdr.linkUuid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.parentUuid = {0}", _vHdr.parentUuid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.logicalCylinders = {0}", _vHdr.logicalCylinders);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.logicalHeads = {0}", _vHdr.logicalHeads);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.logicalSpt = {0}", _vHdr.logicalSpt);
        AaruConsole.DebugWriteLine(MODULE_NAME, "vHdr.logicalSectorSize = {0}", _vHdr.logicalSectorSize);

        if(_vHdr.imageType != VdiImageType.Normal)
        {
            AaruConsole.ErrorWriteLine(string.Format(Localization.Support_for_image_type_0_not_yet_implemented,
                                                     _vHdr.imageType));

            return ErrorNumber.InvalidArgument;
        }

        var blockMapStopwatch = new Stopwatch();
        blockMapStopwatch.Start();
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_Image_Block_Map);
        stream.Seek(_vHdr.offsetBlocks, SeekOrigin.Begin);
        byte[] ibmB = new byte[_vHdr.blocks * 4];
        stream.EnsureRead(ibmB, 0, ibmB.Length);
        _ibm = MemoryMarshal.Cast<byte, uint>(ibmB).ToArray();
        blockMapStopwatch.Stop();

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_Image_Block_Map_took_0_ms,
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

        switch(_vHdr.creator)
        {
            case SUN_VDI:
                _imageInfo.Application = "Sun VirtualBox";

                break;
            case SUN_OLD_VDI:
                _imageInfo.Application = "Sun xVM";

                break;
            case ORACLE_VDI:
                _imageInfo.Application = "Oracle VirtualBox";

                break;
            case QEMUVDI:
                _imageInfo.Application = "QEMU";

                break;
            case INNOTEK_VDI:
            case INNOTEK_OLD_VDI:
                _imageInfo.Application = "innotek VirtualBox";

                break;
            case DIC_VDI:
                _imageInfo.Application = "DiscImageChef";

                break;
            case DIC_AARU:
                _imageInfo.Application = "Aaru";

                break;
        }

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

        if(_imageInfo.Cylinders != 0)
            return ErrorNumber.InvalidArgument;

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
                _imageInfo.Heads = 16;
            }

            _vHdr.logicalCylinders = (uint)(_imageInfo.Sectors / _imageInfo.Heads / _imageInfo.SectorsPerTrack);

            if(_imageInfo.Cylinders == 0 &&
               _imageInfo is { Heads: 0, SectorsPerTrack: 0 })
                break;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(_sectorCache.TryGetValue(sectorAddress, out buffer))
            return ErrorNumber.NoError;

        ulong index  = sectorAddress * _vHdr.sectorSize / _vHdr.blockSize;
        ulong secOff = sectorAddress * _vHdr.sectorSize % _vHdr.blockSize;

        uint ibmOff = _ibm[(int)index];

        if(ibmOff == VDI_EMPTY)
        {
            buffer = new byte[_vHdr.sectorSize];

            return ErrorNumber.NoError;
        }

        ulong imageOff = _vHdr.offsetData + ((ulong)ibmOff * _vHdr.blockSize);

        byte[] cluster = new byte[_vHdr.blockSize];
        _imageStream.Seek((long)imageOff, SeekOrigin.Begin);
        _imageStream.EnsureRead(cluster, 0, (int)_vHdr.blockSize);
        buffer = new byte[_vHdr.sectorSize];
        Array.Copy(cluster, (int)secOff, buffer, 0, _vHdr.sectorSize);

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