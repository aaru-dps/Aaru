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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages
{
    public sealed partial class Vdi
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] vHdrB = new byte[Marshal.SizeOf<Header>()];
            stream.Read(vHdrB, 0, Marshal.SizeOf<Header>());
            _vHdr = Marshal.ByteArrayToStructureLittleEndian<Header>(vHdrB);

            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.creator = {0}", _vHdr.creator);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.magic = {0}", _vHdr.magic);

            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.version = {0}.{1}", _vHdr.majorVersion,
                                       _vHdr.minorVersion);

            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.headerSize = {0}", _vHdr.headerSize);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.imageType = {0}", _vHdr.imageType);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.imageFlags = {0}", _vHdr.imageFlags);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.description = {0}", _vHdr.comments);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.offsetBlocks = {0}", _vHdr.offsetBlocks);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.offsetData = {0}", _vHdr.offsetData);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.cylinders = {0}", _vHdr.cylinders);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.heads = {0}", _vHdr.heads);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.spt = {0}", _vHdr.spt);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.sectorSize = {0}", _vHdr.sectorSize);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.size = {0}", _vHdr.size);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blockSize = {0}", _vHdr.blockSize);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blockExtraData = {0}", _vHdr.blockExtraData);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blocks = {0}", _vHdr.blocks);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.allocatedBlocks = {0}", _vHdr.allocatedBlocks);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.uuid = {0}", _vHdr.uuid);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.snapshotUuid = {0}", _vHdr.snapshotUuid);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.linkUuid = {0}", _vHdr.linkUuid);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.parentUuid = {0}", _vHdr.parentUuid);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.logicalCylinders = {0}", _vHdr.logicalCylinders);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.logicalHeads = {0}", _vHdr.logicalHeads);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.logicalSpt = {0}", _vHdr.logicalSpt);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.logicalSectorSize = {0}", _vHdr.logicalSectorSize);

            if(_vHdr.imageType != VdiImageType.Normal)
                throw new
                    FeatureSupportedButNotImplementedImageException($"Support for image type {_vHdr.imageType} not yet implemented");

            DateTime start = DateTime.UtcNow;
            AaruConsole.DebugWriteLine("VirtualBox plugin", "Reading Image Block Map");
            stream.Seek(_vHdr.offsetBlocks, SeekOrigin.Begin);
            byte[] ibmB = new byte[_vHdr.blocks * 4];
            stream.Read(ibmB, 0, ibmB.Length);
            _ibm = MemoryMarshal.Cast<byte, uint>(ibmB).ToArray();
            DateTime end = DateTime.UtcNow;

            AaruConsole.DebugWriteLine("VirtualBox plugin", "Reading Image Block Map took {0} ms",
                                       (end - start).TotalMilliseconds);

            _sectorCache = new Dictionary<ulong, byte[]>();

            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            _imageInfo.Sectors              = _vHdr.size / _vHdr.sectorSize;
            _imageInfo.ImageSize            = _vHdr.size;
            _imageInfo.SectorSize           = _vHdr.sectorSize;
            _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
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
                return true;

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

                if(_imageInfo.Cylinders       == 0 &&
                   _imageInfo.Heads           == 0 &&
                   _imageInfo.SectorsPerTrack == 0)
                    break;
            }

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(_sectorCache.TryGetValue(sectorAddress, out byte[] sector))
                return sector;

            ulong index  = sectorAddress * _vHdr.sectorSize / _vHdr.blockSize;
            ulong secOff = sectorAddress * _vHdr.sectorSize % _vHdr.blockSize;

            uint ibmOff = _ibm[(int)index];

            if(ibmOff == VDI_EMPTY)
                return new byte[_vHdr.sectorSize];

            ulong imageOff = _vHdr.offsetData + ((ulong)ibmOff * _vHdr.blockSize);

            byte[] cluster = new byte[_vHdr.blockSize];
            _imageStream.Seek((long)imageOff, SeekOrigin.Begin);
            _imageStream.Read(cluster, 0, (int)_vHdr.blockSize);
            sector = new byte[_vHdr.sectorSize];
            Array.Copy(cluster, (int)secOff, sector, 0, _vHdr.sectorSize);

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
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({sectorAddress} + {length}) than available ({_imageInfo.Sectors})");

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}