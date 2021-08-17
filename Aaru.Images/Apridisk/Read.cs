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
//     Reads Apridisk disk images.
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
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class Apridisk
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            // Skip signature
            stream.Seek(_signature.Length, SeekOrigin.Begin);

            int totalCylinders = -1;
            int totalHeads     = -1;
            int maxSector      = -1;
            int recordSize     = Marshal.SizeOf<Record>();

            // Count cylinders
            while(stream.Position < stream.Length)
            {
                byte[] recB = new byte[recordSize];
                stream.Read(recB, 0, recordSize);

                Record record = Marshal.SpanToStructureLittleEndian<Record>(recB);

                switch(record.type)
                {
                    // Deleted record, just skip it
                    case RecordType.Deleted:
                        AaruConsole.DebugWriteLine("Apridisk plugin", "Found deleted record at {0}", stream.Position);
                        stream.Seek((record.headerSize - recordSize) + record.dataSize, SeekOrigin.Current);

                        break;
                    case RecordType.Comment:
                        AaruConsole.DebugWriteLine("Apridisk plugin", "Found comment record at {0}", stream.Position);
                        stream.Seek(record.headerSize - recordSize, SeekOrigin.Current);
                        byte[] commentB = new byte[record.dataSize];
                        stream.Read(commentB, 0, commentB.Length);
                        _imageInfo.Comments = StringHandlers.CToString(commentB);
                        AaruConsole.DebugWriteLine("Apridisk plugin", "Comment: \"{0}\"", _imageInfo.Comments);

                        break;
                    case RecordType.Creator:
                        AaruConsole.DebugWriteLine("Apridisk plugin", "Found creator record at {0}", stream.Position);
                        stream.Seek(record.headerSize - recordSize, SeekOrigin.Current);
                        byte[] creatorB = new byte[record.dataSize];
                        stream.Read(creatorB, 0, creatorB.Length);
                        _imageInfo.Creator = StringHandlers.CToString(creatorB);
                        AaruConsole.DebugWriteLine("Apridisk plugin", "Creator: \"{0}\"", _imageInfo.Creator);

                        break;
                    case RecordType.Sector:
                        if(record.compression != CompressType.Compressed &&
                           record.compression != CompressType.Uncompresed)
                            throw new
                                ImageNotSupportedException($"Found record with unknown compression type 0x{(ushort)record.compression:X4} at {stream.Position}");

                        AaruConsole.DebugWriteLine("Apridisk plugin",
                                                   "Found {4} sector record at {0} for cylinder {1} head {2} sector {3}",
                                                   stream.Position, record.cylinder, record.head, record.sector,
                                                   record.compression == CompressType.Compressed ? "compressed"
                                                       : "uncompressed");

                        if(record.cylinder > totalCylinders)
                            totalCylinders = record.cylinder;

                        if(record.head > totalHeads)
                            totalHeads = record.head;

                        if(record.sector > maxSector)
                            maxSector = record.sector;

                        stream.Seek((record.headerSize - recordSize) + record.dataSize, SeekOrigin.Current);

                        break;
                    default:
                        throw new
                            ImageNotSupportedException($"Found record with unknown type 0x{(uint)record.type:X8} at {stream.Position}");
                }
            }

            totalCylinders++;
            totalHeads++;

            if(totalCylinders <= 0 ||
               totalHeads     <= 0)
                throw new ImageNotSupportedException("No cylinders or heads found");

            _sectorsData = new byte[totalCylinders][][][];

            // Total sectors per track
            uint[][] spts = new uint[totalCylinders][];

            _imageInfo.Cylinders = (ushort)totalCylinders;
            _imageInfo.Heads     = (byte)totalHeads;

            AaruConsole.DebugWriteLine("Apridisk plugin",
                                       "Found {0} cylinders and {1} heads with a maximum sector number of {2}",
                                       totalCylinders, totalHeads, maxSector);

            // Create heads
            for(int i = 0; i < totalCylinders; i++)
            {
                _sectorsData[i] = new byte[totalHeads][][];
                spts[i]         = new uint[totalHeads];

                for(int j = 0; j < totalHeads; j++)
                    _sectorsData[i][j] = new byte[maxSector + 1][];
            }

            _imageInfo.SectorSize = uint.MaxValue;

            ulong headerSizes = 0;

            // Read sectors
            stream.Seek(_signature.Length, SeekOrigin.Begin);

            while(stream.Position < stream.Length)
            {
                byte[] recB = new byte[recordSize];
                stream.Read(recB, 0, recordSize);

                Record record = Marshal.SpanToStructureLittleEndian<Record>(recB);

                switch(record.type)
                {
                    // Not sector record, just skip it
                    case RecordType.Deleted:
                    case RecordType.Comment:
                    case RecordType.Creator:
                        stream.Seek((record.headerSize - recordSize) + record.dataSize, SeekOrigin.Current);
                        headerSizes += record.headerSize + record.dataSize;

                        break;
                    case RecordType.Sector:
                        stream.Seek(record.headerSize - recordSize, SeekOrigin.Current);

                        byte[] data = new byte[record.dataSize];
                        stream.Read(data, 0, data.Length);

                        spts[record.cylinder][record.head]++;
                        uint realLength = record.dataSize;

                        if(record.compression == CompressType.Compressed)
                            realLength =
                                Decompress(data, out _sectorsData[record.cylinder][record.head][record.sector]);
                        else
                            _sectorsData[record.cylinder][record.head][record.sector] = data;

                        if(realLength < _imageInfo.SectorSize)
                            _imageInfo.SectorSize = realLength;

                        headerSizes += record.headerSize + record.dataSize;

                        break;
                }
            }

            AaruConsole.DebugWriteLine("Apridisk plugin", "Found a minimum of {0} bytes per sector",
                                       _imageInfo.SectorSize);

            // Count sectors per track
            uint spt = uint.MaxValue;

            for(ushort cyl = 0; cyl < _imageInfo.Cylinders; cyl++)
            {
                for(ushort head = 0; head < _imageInfo.Heads; head++)
                    if(spts[cyl][head] < spt)
                        spt = spts[cyl][head];
            }

            _imageInfo.SectorsPerTrack = spt;

            AaruConsole.DebugWriteLine("Apridisk plugin", "Found a minimum of {0} sectors per track",
                                       _imageInfo.SectorsPerTrack);

            _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                          (ushort)_imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                          false));

            _imageInfo.ImageSize            = (ulong)stream.Length - headerSizes;
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            _imageInfo.Sectors              = _imageInfo.Cylinders * _imageInfo.Heads * _imageInfo.SectorsPerTrack;
            _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress)
        {
            (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

            if(cylinder >= _sectorsData.Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(head >= _sectorsData[cylinder].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sector > _sectorsData[cylinder][head].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            return _sectorsData[cylinder][head][sector];
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var buffer = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                buffer.Write(sector, 0, sector.Length);
            }

            return buffer.ToArray();
        }
    }
}