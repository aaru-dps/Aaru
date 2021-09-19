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
//     Reads HD-Copy disk images.
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
// Copyright © 2017 Michael Drüing
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
    public sealed partial class HdCopy
    {
        /// <inheritdoc />
        public ErrorNumber Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            var fheader = new FileHeader();

            // the start offset of the track data
            long currentOffset = 0;

            if(!TryReadHeader(stream, ref fheader, ref currentOffset))
                return ErrorNumber.InvalidArgument;

            AaruConsole.DebugWriteLine("HDCP plugin",
                                       "Detected HD-Copy image with {0} tracks and {1} sectors per track.",
                                       fheader.lastCylinder + 1, fheader.sectorsPerTrack);

            _imageInfo.Cylinders       = (uint)fheader.lastCylinder + 1;
            _imageInfo.SectorsPerTrack = fheader.sectorsPerTrack;
            _imageInfo.SectorSize      = 512; // only 512 bytes per sector supported
            _imageInfo.Heads           = 2;   // only 2-sided floppies are supported
            _imageInfo.Sectors         = 2                  * _imageInfo.Cylinders * _imageInfo.SectorsPerTrack;
            _imageInfo.ImageSize       = _imageInfo.Sectors * _imageInfo.SectorSize;

            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            _imageInfo.CreationTime         = imageFilter.CreationTime;
            _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);

            _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, 2,
                                                          (ushort)_imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                          false));

            // build table of track offsets
            for(int i = 0; i < _imageInfo.Cylinders * 2; i++)
                if(fheader.trackMap[i] == 0)
                    _trackOffset[i] = -1;
                else
                {
                    // track is present, read the block header
                    if(currentOffset + 3 >= stream.Length)
                        return ErrorNumber.InvalidArgument;

                    byte[] blkHeader = new byte[2];
                    stream.Read(blkHeader, 0, 2);
                    short blkLength = BitConverter.ToInt16(blkHeader, 0);

                    // assume block sizes are positive
                    if(blkLength < 0)
                        return ErrorNumber.InvalidArgument;

                    AaruConsole.DebugWriteLine("HDCP plugin", "Track {0} offset 0x{1:x8}, size={2:x4}", i,
                                               currentOffset, blkLength);

                    _trackOffset[i] = currentOffset;

                    currentOffset += 2 + blkLength;

                    // skip the block data
                    stream.Seek(blkLength, SeekOrigin.Current);
                }

            // ensure that the last track is present completely
            if(currentOffset > stream.Length)
                return ErrorNumber.InvalidArgument;

            // save some variables for later use
            _fileHeader      = fheader;
            _hdcpImageFilter = imageFilter;

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
        {
            buffer = null;
            int trackNum     = (int)(sectorAddress / _imageInfo.SectorsPerTrack);
            int sectorOffset = (int)(sectorAddress % _imageInfo.SectorsPerTrack);

            if(sectorAddress > _imageInfo.Sectors - 1)
                return ErrorNumber.OutOfRange;

            if(trackNum > 2 * _imageInfo.Cylinders)
                return ErrorNumber.SectorNotFound;

            buffer = new byte[_imageInfo.SectorSize];

            if(_trackOffset[trackNum] == -1)
                Array.Clear(buffer, 0, (int)_imageInfo.SectorSize);
            else
            {
                // track is present in file, make sure it has been loaded
                if(!_trackCache.ContainsKey(trackNum))
                    ReadTrackIntoCache(_hdcpImageFilter.GetDataForkStream(), trackNum);

                Array.Copy(_trackCache[trackNum], sectorOffset * _imageInfo.SectorSize, buffer, 0,
                           _imageInfo.SectorSize);
            }

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
}