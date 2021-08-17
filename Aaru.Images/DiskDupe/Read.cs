// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads DiskDupe DDI disk images.
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
// Copyright © 2021 Michael Drüing
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;

namespace Aaru.DiscImages
{
    public sealed partial class DiskDupe
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            FileHeader fHeader = new FileHeader();
            TrackInfo[] trackMap = null;
            long[] trackOffsets = null;

            if (!TryReadHeader(stream, ref fHeader, ref trackMap, ref trackOffsets))
            {
                return false;
            }

            AaruConsole.DebugWriteLine("DiskDupe Plugin",
                                       "Detected DiskDupe DDI image with {0} tracks and {1} sectors per track.",
                                       _diskTypes[fHeader.diskType].cyl, _diskTypes[fHeader.diskType].spt);

            _imageInfo.Cylinders       = _diskTypes[fHeader.diskType].cyl;
            _imageInfo.Heads           = _diskTypes[fHeader.diskType].hd;
            _imageInfo.SectorsPerTrack = _diskTypes[fHeader.diskType].spt;
            _imageInfo.SectorSize      = 512; // only 512 bytes per sector supported
            _imageInfo.Sectors         = _imageInfo.Heads * _imageInfo.Cylinders * _imageInfo.SectorsPerTrack;
            _imageInfo.ImageSize       = _imageInfo.Sectors * _imageInfo.SectorSize;

            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());

            _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                          (ushort)_imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                          false));


            // save some variables for later use
            _fileHeader     = fHeader;
            _ddiImageFilter = imageFilter;
            _trackMap       = trackMap;
            _trackOffsets   = trackOffsets;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            int trackNum     = (int)(sectorAddress / _imageInfo.SectorsPerTrack);
            int sectorOffset = (int)(sectorAddress % _imageInfo.SectorsPerTrack);

            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(trackNum > 2 * _imageInfo.Cylinders)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            byte[] result = new byte[_imageInfo.SectorSize];

            if(_trackMap[trackNum].present != 1)
                Array.Clear(result, 0, (int)_imageInfo.SectorSize);
            else
            {
                Stream strm = _ddiImageFilter.GetDataForkStream();

                strm.Seek(_trackOffsets[trackNum] + sectorOffset * _imageInfo.SectorSize, SeekOrigin.Begin);

                strm.Read(result, 0, (int)_imageInfo.SectorSize);
            }

            return result;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint count)
        {
            byte[] result = new byte[count * _imageInfo.SectorSize];

            if(sectorAddress + count > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(count), "Requested more sectors than available");

            for(int i = 0; i < count; i++)
                ReadSector(sectorAddress + (ulong)i).CopyTo(result, i * _imageInfo.SectorSize);

            return result;
        }
    }
}