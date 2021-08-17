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
//     Reads Apple nibbelized disk images.
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
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Decoders.Floppy;

namespace Aaru.DiscImages
{
    public sealed partial class AppleNib
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            AaruConsole.DebugWriteLine("Apple NIB Plugin", "Decoding whole image");
            List<Apple2.RawTrack> tracks = Apple2.MarshalDisk(buffer);
            AaruConsole.DebugWriteLine("Apple NIB Plugin", "Got {0} tracks", tracks.Count);

            Dictionary<ulong, Apple2.RawSector> rawSectors = new Dictionary<ulong, Apple2.RawSector>();

            int  spt            = 0;
            bool allTracksEqual = true;

            for(int i = 1; i < tracks.Count; i++)
                allTracksEqual &= tracks[i - 1].sectors.Length == tracks[i].sectors.Length;

            if(allTracksEqual)
                spt = tracks[0].sectors.Length;

            bool    skewed  = spt == 16;
            ulong[] skewing = _proDosSkewing;

            // Detect ProDOS skewed disks
            if(skewed)
                foreach(bool isDos in from sector in tracks[17].sectors
                                      where sector.addressField.sector.SequenceEqual(new byte[]
                                      {
                                          170, 170
                                      }) select Apple2.DecodeSector(sector) into sector0 where sector0 != null
                                      select sector0[0x01] == 17 && sector0[0x02] < 16  && sector0[0x27] <= 122 &&
                                             sector0[0x34] == 35 && sector0[0x35] == 16 && sector0[0x36] == 0   &&
                                             sector0[0x37] == 1)
                {
                    if(isDos)
                        skewing = _dosSkewing;

                    AaruConsole.DebugWriteLine("Apple NIB Plugin", "Using {0}DOS skewing",
                                               skewing.SequenceEqual(_dosSkewing) ? "" : "Pro");
                }

            for(int i = 0; i < tracks.Count; i++)
                foreach(Apple2.RawSector sector in tracks[i].sectors)
                    if(skewed && spt != 0)
                    {
                        ulong sectorNo = (ulong)((((sector.addressField.sector[0] & 0x55) << 1) |
                                                  (sector.addressField.sector[1] & 0x55)) & 0xFF);

                        AaruConsole.DebugWriteLine("Apple NIB Plugin",
                                                   "Hardware sector {0} of track {1} goes to logical sector {2}",
                                                   sectorNo, i, skewing[sectorNo] + (ulong)(i * spt));

                        rawSectors.Add(skewing[sectorNo] + (ulong)(i * spt), sector);
                        _imageInfo.Sectors++;
                    }
                    else
                    {
                        rawSectors.Add(_imageInfo.Sectors, sector);
                        _imageInfo.Sectors++;
                    }

            AaruConsole.DebugWriteLine("Apple NIB Plugin", "Got {0} sectors", _imageInfo.Sectors);

            AaruConsole.DebugWriteLine("Apple NIB Plugin", "Cooking sectors");

            _longSectors   = new Dictionary<ulong, byte[]>();
            _cookedSectors = new Dictionary<ulong, byte[]>();
            _addressFields = new Dictionary<ulong, byte[]>();

            foreach(KeyValuePair<ulong, Apple2.RawSector> kvp in rawSectors)
            {
                byte[] cooked = Apple2.DecodeSector(kvp.Value);
                byte[] raw    = Apple2.MarshalSector(kvp.Value);
                byte[] addr   = Apple2.MarshalAddressField(kvp.Value.addressField);
                _longSectors.Add(kvp.Key, raw);
                _cookedSectors.Add(kvp.Key, cooked);
                _addressFields.Add(kvp.Key, addr);
            }

            _imageInfo.ImageSize            = (ulong)imageFilter.GetDataForkLength();
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());

            if(_imageInfo.Sectors == 455)
                _imageInfo.MediaType = MediaType.Apple32SS;
            else if(_imageInfo.Sectors == 560)
                _imageInfo.MediaType = MediaType.Apple33SS;
            else
                _imageInfo.MediaType = MediaType.Unknown;

            _imageInfo.SectorSize   = 256;
            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            _imageInfo.ReadableSectorTags.Add(SectorTagType.FloppyAddressMark);

            switch(_imageInfo.MediaType)
            {
                case MediaType.Apple32SS:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 13;

                    break;
                case MediaType.Apple33SS:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 16;

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

            _cookedSectors.TryGetValue(sectorAddress, out byte[] temp);

            return temp;
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(tag != SectorTagType.FloppyAddressMark)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            _addressFields.TryGetValue(sectorAddress, out byte[] temp);

            return temp;
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            if(tag != SectorTagType.FloppyAddressMark)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSectorTag(sectorAddress + i, tag);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            _longSectors.TryGetValue(sectorAddress, out byte[] temp);

            return temp;
        }

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSectorLong(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}