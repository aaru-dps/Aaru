// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleNIB.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Apple nibbelized disc images.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Decoders.Floppy;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    // TODO: Checksum sectors
    public class AppleNib : IMediaImage
    {
        readonly byte[] apple3_sign = {0x8D, 0xD0, 0x03, 0x4C, 0xC7, 0xA4};
        readonly byte[] cpm_sign = {0xA2, 0x55, 0xA9, 0x00, 0x9D, 0x00, 0x0D, 0xCA};
        readonly byte[] dos_sign = {0xA2, 0x02, 0x8E, 0x52};
        readonly ulong[] dosSkewing = {0, 7, 14, 6, 13, 5, 12, 4, 11, 3, 10, 2, 9, 1, 8, 15};
        readonly byte[] dri_string =
        {
            0x43, 0x4F, 0x50, 0x59, 0x52, 0x49, 0x47, 0x48, 0x54, 0x20, 0x28, 0x43, 0x29, 0x20, 0x31, 0x39, 0x37, 0x39,
            0x2C, 0x20, 0x44, 0x49, 0x47, 0x49, 0x54, 0x41, 0x4C, 0x20, 0x52, 0x45, 0x53, 0x45, 0x41, 0x52, 0x43, 0x48
        };
        readonly byte[] pascal_sign = {0x08, 0xA5, 0x0F, 0x29};
        readonly byte[] pascal_string = {0x53, 0x59, 0x53, 0x54, 0x45, 0x2E, 0x41, 0x50, 0x50, 0x4C, 0x45};
        readonly byte[] pascal2_sign = {0xFF, 0xA2, 0x00, 0x8E};
        readonly byte[] prodos_string = {0x50, 0x52, 0x4F, 0x44, 0x4F, 0x53};
        readonly ulong[] proDosSkewing = {0, 8, 1, 9, 2, 10, 3, 11, 4, 12, 5, 13, 6, 14, 7, 15};
        readonly byte[] sos_sign = {0xC9, 0x20, 0xF0, 0x3E};
        Dictionary<ulong, byte[]> addressFields;
        Dictionary<ulong, byte[]> cookedSectors;
        ImageInfo imageInfo;
        Dictionary<ulong, byte[]> longSectors;

        public AppleNib()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                HasPartitions = false,
                HasSessions = false,
                Version = null,
                Application = null,
                ApplicationVersion = null,
                Creator = null,
                Comments = null,
                MediaManufacturer = null,
                MediaModel = null,
                MediaSerialNumber = null,
                MediaBarcode = null,
                MediaPartNumber = null,
                MediaSequence = 0,
                LastMediaSequence = 0,
                DriveManufacturer = null,
                DriveModel = null,
                DriveSerialNumber = null,
                DriveFirmwareRevision = null
            };
        }

        public ImageInfo Info => imageInfo;

        public string Name => "Apple NIB";
        public Guid Id => new Guid("AE171AE8-6747-49CC-B861-9D450B7CD42E");

        public string ImageFormat => "Apple nibbles";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool IdentifyImage(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] test = new byte[512];
            stream.Read(test, 0, 512);

            return Apple2.IsApple2GCR(test);
        }

        public bool OpenImage(IFilter imageFilter)
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

            int spt = 0;
            bool allTracksEqual = true;
            for(int i = 1; i < tracks.Count; i++)
                allTracksEqual &= tracks[i - 1].sectors.Length == tracks[i].sectors.Length;

            if(allTracksEqual) spt = tracks[0].sectors.Length;

            bool skewed = spt == 16;
            ulong[] skewing = dosSkewing;

            // Detect ProDOS skewed disks
            if(skewed)
            {
                byte[] sector1 = null;
                byte[] sector0 = null;

                foreach(Apple2.RawSector sector in tracks[0].sectors)
                {
                    if(sector.addressField.sector.SequenceEqual(new byte[] {170, 171}))
                        sector1 = Apple2.DecodeSector(sector);
                    if(sector.addressField.sector.SequenceEqual(new byte[] {170, 170}))
                        sector0 = Apple2.DecodeSector(sector);
                }

                if(sector1 != null)
                {
                    byte[] tmpAt0Sz4 = new byte[4];
                    byte[] tmpAt0Sz6 = new byte[6];
                    byte[] tmpAt0Sz8 = new byte[8];
                    byte[] tmpAt3Sz6 = new byte[6];
                    byte[] tmpAt24Sz36 = new byte[36];
                    byte[] tmpAt33Sz6 = new byte[6];

                    Array.Copy(sector1, 0, tmpAt0Sz4, 0, 4);
                    Array.Copy(sector1, 0, tmpAt0Sz6, 0, 6);
                    Array.Copy(sector1, 0, tmpAt0Sz8, 0, 8);
                    Array.Copy(sector1, 3, tmpAt3Sz6, 0, 6);
                    Array.Copy(sector1, 24, tmpAt24Sz36, 0, 36);
                    Array.Copy(sector1, 33, tmpAt33Sz6, 0, 6);

                    if(tmpAt0Sz4.SequenceEqual(sos_sign)) skewing = proDosSkewing;
                    if(tmpAt0Sz4.SequenceEqual(dos_sign)) skewing = proDosSkewing;
                    if(tmpAt0Sz4.SequenceEqual(pascal2_sign)) skewing = proDosSkewing;
                    if(tmpAt0Sz6.SequenceEqual(apple3_sign)) skewing = proDosSkewing;
                    if(tmpAt0Sz8.SequenceEqual(cpm_sign)) skewing = proDosSkewing;
                    if(tmpAt3Sz6.SequenceEqual(prodos_string)) skewing = proDosSkewing;
                    if(tmpAt24Sz36.SequenceEqual(dri_string)) skewing = proDosSkewing;
                    if(tmpAt33Sz6.SequenceEqual(prodos_string)) skewing = proDosSkewing;

                    if(sector0 != null)
                    {
                        byte[] tmpAt215Sz12 = new byte[12];
                        Array.Copy(sector0, 215, tmpAt215Sz12, 0, 12);
                        if(tmpAt215Sz12.SequenceEqual(pascal_string) && tmpAt0Sz4.SequenceEqual(pascal_sign))
                            skewing = proDosSkewing;
                    }

                    DicConsole.DebugWriteLine("Apple NIB Plugin", "Image is skewed");
                }
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

            longSectors = new Dictionary<ulong, byte[]>();
            cookedSectors = new Dictionary<ulong, byte[]>();
            addressFields = new Dictionary<ulong, byte[]>();

            foreach(KeyValuePair<ulong, Apple2.RawSector> kvp in rawSectors)
            {
                byte[] cooked = Apple2.DecodeSector(kvp.Value);
                byte[] raw = Apple2.MarshalSector(kvp.Value);
                byte[] addr = Apple2.MarshalAddressField(kvp.Value.addressField);
                longSectors.Add(kvp.Key, raw);
                cookedSectors.Add(kvp.Key, cooked);
                addressFields.Add(kvp.Key, addr);
            }

            imageInfo.ImageSize = (ulong)imageFilter.GetDataForkLength();
            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            if(imageInfo.Sectors == 455) imageInfo.MediaType = MediaType.Apple32SS;
            else if(imageInfo.Sectors == 560) imageInfo.MediaType = MediaType.Apple33SS;
            else imageInfo.MediaType = MediaType.Unknown;
            imageInfo.SectorSize = 256;
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            imageInfo.ReadableSectorTags.Add(SectorTagType.FloppyAddressMark);
            switch(imageInfo.MediaType)
            {
                case MediaType.Apple32SS:
                    imageInfo.Cylinders = 35;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 13;
                    break;
                case MediaType.Apple33SS:
                    imageInfo.Cylinders = 35;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 16;
                    break;
            }

            return true;
        }

        MediaType GetMediaType()
        {
            switch(imageInfo.Sectors)
            {
                case 455: return MediaType.Apple32SS;
                case 560: return MediaType.Apple33SS;
                default: return MediaType.Unknown;
            }
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

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }
    }
}