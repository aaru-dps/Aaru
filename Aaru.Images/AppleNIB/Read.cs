// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
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
    public partial class AppleNib
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            DicConsole.DebugWriteLine("Apple NIB Plugin", "Decoding whole image");
            List<Apple2.RawTrack> tracks = Apple2.MarshalDisk(buffer);
            DicConsole.DebugWriteLine("Apple NIB Plugin", "Got {0} tracks", tracks.Count);

            Dictionary<ulong, Apple2.RawSector> rawSectors = new Dictionary<ulong, Apple2.RawSector>();

            int  spt            = 0;
            bool allTracksEqual = true;
            for(int i = 1; i < tracks.Count; i++)
                allTracksEqual &= tracks[i - 1].sectors.Length == tracks[i].sectors.Length;

            if(allTracksEqual) spt = tracks[0].sectors.Length;

            bool    skewed  = spt == 16;
            ulong[] skewing = proDosSkewing;

            // Detect ProDOS skewed disks
            if(skewed)
                foreach(Apple2.RawSector sector in tracks[17].sectors)
                {
                    if(!sector.addressField.sector.SequenceEqual(new byte[] {170, 170})) continue;

                    byte[] sector0 = Apple2.DecodeSector(sector);

                    if(sector0 == null) continue;

                    bool isDos = sector0[0x01] == 17 && sector0[0x02] < 16  && sector0[0x27] <= 122 &&
                                 sector0[0x34] == 35 && sector0[0x35] == 16 && sector0[0x36] == 0   &&
                                 sector0[0x37] == 1;

                    if(isDos) skewing = dosSkewing;

                    DicConsole.DebugWriteLine("Apple NIB Plugin", "Using {0}DOS skewing",
                                              skewing.SequenceEqual(dosSkewing) ? "" : "Pro");
                }

            for(int i = 0; i < tracks.Count; i++)
                foreach(Apple2.RawSector sector in tracks[i].sectors)
                    if(skewed && spt != 0)
                    {
                        ulong sectorNo = (ulong)((((sector.addressField.sector[0] & 0x55) << 1) |
                                                  (sector.addressField.sector[1] & 0x55)) & 0xFF);
                        DicConsole.DebugWriteLine("Apple NIB Plugin",
                                                  "Hardware sector {0} of track {1} goes to logical sector {2}",
                                                  sectorNo, i, skewing[sectorNo] + (ulong)(i * spt));
                        rawSectors.Add(skewing[sectorNo] + (ulong)(i * spt), sector);
                        imageInfo.Sectors++;
                    }
                    else
                    {
                        rawSectors.Add(imageInfo.Sectors, sector);
                        imageInfo.Sectors++;
                    }

            DicConsole.DebugWriteLine("Apple NIB Plugin", "Got {0} sectors", imageInfo.Sectors);

            DicConsole.DebugWriteLine("Apple NIB Plugin", "Cooking sectors");

            longSectors   = new Dictionary<ulong, byte[]>();
            cookedSectors = new Dictionary<ulong, byte[]>();
            addressFields = new Dictionary<ulong, byte[]>();

            foreach(KeyValuePair<ulong, Apple2.RawSector> kvp in rawSectors)
            {
                byte[] cooked = Apple2.DecodeSector(kvp.Value);
                byte[] raw    = Apple2.MarshalSector(kvp.Value);
                byte[] addr   = Apple2.MarshalAddressField(kvp.Value.addressField);
                longSectors.Add(kvp.Key, raw);
                cookedSectors.Add(kvp.Key, cooked);
                addressFields.Add(kvp.Key, addr);
            }

            imageInfo.ImageSize            = (ulong)imageFilter.GetDataForkLength();
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            if(imageInfo.Sectors      == 455) imageInfo.MediaType = MediaType.Apple32SS;
            else if(imageInfo.Sectors == 560) imageInfo.MediaType = MediaType.Apple33SS;
            else imageInfo.MediaType                              = MediaType.Unknown;
            imageInfo.SectorSize   = 256;
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            imageInfo.ReadableSectorTags.Add(SectorTagType.FloppyAddressMark);
            switch(imageInfo.MediaType)
            {
                case MediaType.Apple32SS:
                    imageInfo.Cylinders       = 35;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 13;
                    break;
                case MediaType.Apple33SS:
                    imageInfo.Cylinders       = 35;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 16;
                    break;
            }

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            cookedSectors.TryGetValue(sectorAddress, out byte[] temp);
            return temp;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(tag != SectorTagType.FloppyAddressMark)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            addressFields.TryGetValue(sectorAddress, out byte[] temp);
            return temp;
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            if(tag != SectorTagType.FloppyAddressMark)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSectorTag(sectorAddress + i, tag);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            longSectors.TryGetValue(sectorAddress, out byte[] temp);
            return temp;
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSectorLong(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}