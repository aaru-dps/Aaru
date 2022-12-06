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
//     Reads d2f disk images.
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
// Copyright © 2018-2023 Michael Drüing
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class WCDiskImage
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            string comments = string.Empty;
            Stream stream   = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] header = new byte[32];
            stream.Read(header, 0, 32);

            FileHeader fheader = Marshal.ByteArrayToStructureLittleEndian<FileHeader>(header);

            AaruConsole.DebugWriteLine("d2f plugin",
                                       "Detected WC DISK IMAGE with {0} heads, {1} tracks and {2} sectors per track.",
                                       fheader.heads, fheader.cylinders, fheader.sectorsPerTrack);

            _imageInfo.Cylinders       = fheader.cylinders;
            _imageInfo.SectorsPerTrack = fheader.sectorsPerTrack;
            _imageInfo.SectorSize      = 512; // only 512 bytes per sector supported
            _imageInfo.Heads           = fheader.heads;
            _imageInfo.Sectors         = _imageInfo.Heads   * _imageInfo.Cylinders * _imageInfo.SectorsPerTrack;
            _imageInfo.ImageSize       = _imageInfo.Sectors * _imageInfo.SectorSize;

            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());

            _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                          (ushort)_imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                          false));

            /* buffer the entire disk in memory */
            for(int cyl = 0; cyl < _imageInfo.Cylinders; cyl++)
            {
                for(int head = 0; head < _imageInfo.Heads; head++)
                    ReadTrack(stream, cyl, head);
            }

            /* if there are extra tracks, read them as well */
            if(fheader.extraTracks[0] == 1)
            {
                AaruConsole.DebugWriteLine("d2f plugin", "Extra track 1 (head 0) present, reading");
                ReadTrack(stream, (int)_imageInfo.Cylinders, 0);
            }

            if(fheader.extraTracks[1] == 1)
            {
                AaruConsole.DebugWriteLine("d2f plugin", "Extra track 1 (head 1) present, reading");
                ReadTrack(stream, (int)_imageInfo.Cylinders, 1);
            }

            if(fheader.extraTracks[2] == 1)
            {
                AaruConsole.DebugWriteLine("d2f plugin", "Extra track 2 (head 0) present, reading");
                ReadTrack(stream, (int)_imageInfo.Cylinders + 1, 0);
            }

            if(fheader.extraTracks[3] == 1)
            {
                AaruConsole.DebugWriteLine("d2f plugin", "Extra track 2 (head 1) present, reading");
                ReadTrack(stream, (int)_imageInfo.Cylinders + 1, 1);
            }

            /* adjust number of cylinders */
            if(fheader.extraTracks[0] == 1 ||
               fheader.extraTracks[1] == 1)
                _imageInfo.Cylinders++;

            if(fheader.extraTracks[2] == 1 ||
               fheader.extraTracks[3] == 1)
                _imageInfo.Cylinders++;

            /* read the comment and directory data if present */
            if(fheader.extraFlags.HasFlag(ExtraFlag.Comment))
            {
                AaruConsole.DebugWriteLine("d2f plugin", "Comment present, reading");
                byte[] sheaderBuffer = new byte[6];
                stream.Read(sheaderBuffer, 0, 6);

                SectorHeader sheader = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(sheaderBuffer);

                if(sheader.flag != SectorFlag.Comment)
                    throw new InvalidDataException($"Invalid sector type '{sheader.flag.ToString()}' encountered");

                byte[] comm = new byte[sheader.crc];
                stream.Read(comm, 0, sheader.crc);
                comments += Encoding.ASCII.GetString(comm) + Environment.NewLine;
            }

            if(fheader.extraFlags.HasFlag(ExtraFlag.Directory))
            {
                AaruConsole.DebugWriteLine("d2f plugin", "Directory listing present, reading");
                byte[] sheaderBuffer = new byte[6];
                stream.Read(sheaderBuffer, 0, 6);

                SectorHeader sheader = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(sheaderBuffer);

                if(sheader.flag != SectorFlag.Directory)
                    throw new InvalidDataException($"Invalid sector type '{sheader.flag.ToString()}' encountered");

                byte[] dir = new byte[sheader.crc];
                stream.Read(dir, 0, sheader.crc);
                comments += Encoding.ASCII.GetString(dir);
            }

            if(comments.Length > 0)
                _imageInfo.Comments = comments;

            // save some variables for later use
            _fileHeader    = fheader;
            _wcImageFilter = imageFilter;

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress)
        {
            int sectorNumber   = (int)(sectorAddress % _imageInfo.SectorsPerTrack) + 1;
            int trackNumber    = (int)(sectorAddress / _imageInfo.SectorsPerTrack);
            int headNumber     = _imageInfo.Heads > 1 ? trackNumber % 2 : 0;
            int cylinderNumber = _imageInfo.Heads > 1 ? trackNumber / 2 : trackNumber;

            if(_badSectors[(cylinderNumber, headNumber, sectorNumber)])
            {
                AaruConsole.DebugWriteLine("d2f plugin", "reading bad sector {0} ({1},{2},{3})", sectorAddress,
                                           cylinderNumber, headNumber, sectorNumber);

                /* if we have sector data, return that */
                if(_sectorCache.ContainsKey((cylinderNumber, headNumber, sectorNumber)))
                    return _sectorCache[(cylinderNumber, headNumber, sectorNumber)];

                /* otherwise, return an empty sector */
                return new byte[512];
            }

            return _sectorCache[(cylinderNumber, headNumber, sectorNumber)];
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            byte[] result = new byte[length * _imageInfo.SectorSize];

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            for(int i = 0; i < length; i++)
                ReadSector(sectorAddress + (ulong)i).CopyTo(result, i * _imageInfo.SectorSize);

            return result;
        }

        /// <summary>Read a whole track and cache it</summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="cyl">The cylinder number of the track being read.</param>
        /// <param name="head">The head number of the track being read.</param>
        void ReadTrack(Stream stream, int cyl, int head)
        {
            byte[] sectorData;
            byte[] crc;
            short  calculatedCRC;

            for(int sect = 1; sect < _imageInfo.SectorsPerTrack + 1; sect++)
            {
                /* read the sector header */
                byte[] sheaderBuffer = new byte[6];
                stream.Read(sheaderBuffer, 0, 6);

                SectorHeader sheader = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(sheaderBuffer);

                /* validate the sector header */
                if(sheader.cylinder != cyl  ||
                   sheader.head     != head ||
                   sheader.sector   != sect)
                    throw new
                        InvalidDataException($"Unexpected sector encountered. Found CHS {sheader.cylinder},{sheader.head},{sheader.sector} but expected {cyl},{head},{sect}");

                sectorData = new byte[512];

                /* read the sector data */
                switch(sheader.flag)
                {
                    case SectorFlag.Normal: /* read a normal sector and store it in cache */
                        stream.Read(sectorData, 0, 512);
                        _sectorCache[(cyl, head, sect)] = sectorData;
                        CRC16IBMContext.Data(sectorData, 512, out crc);
                        calculatedCRC = (short)((256 * crc[0]) | crc[1]);
                        /*
                        AaruConsole.DebugWriteLine("d2f plugin", "CHS {0},{1},{2}: Regular sector, stored CRC=0x{3:x4}, calculated CRC=0x{4:x4}",
                            cyl, head, sect, sheader.crc, 256 * crc[0] + crc[1]);
                         */
                        _badSectors[(cyl, head, sect)] = sheader.crc != calculatedCRC;

                        if(calculatedCRC != sheader.crc)
                            AaruConsole.DebugWriteLine("d2f plugin",
                                                       "CHS {0},{1},{2}: CRC mismatch: stored CRC=0x{3:x4}, calculated CRC=0x{4:x4}",
                                                       cyl, head, sect, sheader.crc, calculatedCRC);

                        break;
                    case SectorFlag.BadSector:
                        /*
                        AaruConsole.DebugWriteLine("d2f plugin", "CHS {0},{1},{2}: Bad sector",
                            cyl, head, sect);
                         */
                        _badSectors[(cyl, head, sect)] = true;

                        break;
                    case SectorFlag.RepeatByte:
                        /*
                        AaruConsole.DebugWriteLine("d2f plugin", "CHS {0},{1},{2}: RepeatByte sector, fill byte 0x{0:x2}",
                            cyl, head, sect, sheader.crc & 0xff);
                         */
                        for(int i = 0; i < 512; i++)
                            sectorData[i] = (byte)(sheader.crc & 0xff);

                        _sectorCache[(cyl, head, sect)] = sectorData;
                        _badSectors[(cyl, head, sect)]  = false;

                        break;
                    default:
                        throw new InvalidDataException($"Invalid sector type '{sheader.flag.ToString()}' encountered");
                }
            }
        }
    }
}