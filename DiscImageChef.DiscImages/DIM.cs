// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DIM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages DIM disk images.
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
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class Dim : IMediaImage
    {
        /// <summary>Start of data sectors in disk image, should be 0x100</summary>
        const uint DATA_OFFSET = 0x100;

        readonly byte[] headerId = {0x44, 0x49, 0x46, 0x43, 0x20, 0x48, 0x45, 0x41, 0x44, 0x45, 0x52, 0x20, 0x20};
        byte[] comment;
        /// <summary>Disk image file</summary>
        IFilter dimImageFilter;
        DiskType dskType;
        byte[] hdrId;
        ImageInfo imageInfo;

        public Dim()
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

        public virtual string Name => "DIM Disk Image";
        public virtual Guid Id => new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
        public virtual ImageInfo Info => imageInfo;

        public virtual string ImageFormat => "DIM disk image";

        public virtual List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public virtual List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public virtual List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public virtual bool IdentifyImage(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < DATA_OFFSET) return false;

            comment = new byte[60];
            hdrId = new byte[13];
            stream.Seek(0, SeekOrigin.Begin);
            dskType = (DiskType)stream.ReadByte();
            stream.Seek(0xAB, SeekOrigin.Begin);
            stream.Read(hdrId, 0, 13);
            stream.Seek(0xC2, SeekOrigin.Begin);
            stream.Read(comment, 0, 60);

            return headerId.SequenceEqual(hdrId);
        }

        public virtual bool OpenImage(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < DATA_OFFSET) return false;

            long diskSize = stream.Length - DATA_OFFSET;

            comment = new byte[60];
            hdrId = new byte[13];
            stream.Seek(0, SeekOrigin.Begin);
            dskType = (DiskType)stream.ReadByte();
            stream.Seek(0xAB, SeekOrigin.Begin);
            stream.Read(hdrId, 0, 13);
            stream.Seek(0xC2, SeekOrigin.Begin);
            stream.Read(comment, 0, 60);

            if(!headerId.SequenceEqual(hdrId)) return false;

            imageInfo.MediaType = MediaType.Unknown;

            switch(dskType)
            {
                // 8 spt, 1024 bps
                case DiskType.Hd2:
                    if(diskSize % (2 * 8 * 1024) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 8 * 1024));
                        return false;
                    }

                    if(diskSize / (2 * 8 * 1024) == 77) imageInfo.MediaType = MediaType.SHARP_525;
                    imageInfo.SectorSize = 1024;
                    break;
                // 9 spt, 1024 bps
                case DiskType.Hs2:
                    if(diskSize % (2 * 9 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 9 * 512));
                        return false;
                    }

                    if(diskSize / (2 * 9 * 512) == 80) imageInfo.MediaType = MediaType.SHARP_525_9;
                    imageInfo.SectorSize = 512;
                    break;
                // 15 spt, 512 bps
                case DiskType.Hc2:
                    if(diskSize % (2 * 15 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 15 * 512));
                        return false;
                    }

                    if(diskSize / (2 * 15 * 512) == 80) imageInfo.MediaType = MediaType.DOS_525_HD;
                    imageInfo.SectorSize = 512;
                    break;
                // 9 spt, 1024 bps
                case DiskType.Hde2:
                    if(diskSize % (2 * 9 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 9 * 512));
                        return false;
                    }

                    if(diskSize / (2 * 9 * 512) == 80) imageInfo.MediaType = MediaType.SHARP_35_9;
                    imageInfo.SectorSize = 512;
                    break;
                // 18 spt, 512 bps
                case DiskType.Hq2:
                    if(diskSize % (2 * 18 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 18 * 512));
                        return false;
                    }

                    if(diskSize / (2 * 18 * 512) == 80) imageInfo.MediaType = MediaType.DOS_35_HD;
                    imageInfo.SectorSize = 512;
                    break;
                // 26 spt, 256 bps
                case DiskType.N88:
                    if(diskSize % (2 * 26 * 256) == 0)
                    {
                        if(diskSize % (2 * 26 * 256) == 77) imageInfo.MediaType = MediaType.NEC_8_DD;
                        imageInfo.SectorSize = 256;
                    }
                    else if(diskSize % (2 * 26 * 128) == 0)
                    {
                        if(diskSize % (2 * 26 * 128) == 77) imageInfo.MediaType = MediaType.NEC_8_SD;
                        imageInfo.SectorSize = 256;
                    }
                    else
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 26 * 256));
                        return false;
                    }

                    break;
                default: return false;
            }

            DicConsole.VerboseWriteLine("DIM image contains a disk of type {0}", imageInfo.MediaType);
            if(!string.IsNullOrEmpty(imageInfo.Comments))
                DicConsole.VerboseWriteLine("DIM comments: {0}", imageInfo.Comments);

            dimImageFilter = imageFilter;

            imageInfo.ImageSize = (ulong)diskSize;
            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors = imageInfo.ImageSize / imageInfo.SectorSize;
            imageInfo.Comments = StringHandlers.CToString(comment, Encoding.GetEncoding(932));
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            switch(imageInfo.MediaType)
            {
                case MediaType.SHARP_525:
                    imageInfo.Cylinders = 77;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.SHARP_525_9:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_525_HD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.SHARP_35_9:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_35_HD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 18;
                    break;
                case MediaType.NEC_8_DD:
                case MediaType.NEC_8_SD:
                    imageInfo.Cylinders = 77;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 26;
                    break;
            }

            return true;
        }

        public virtual byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public virtual byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Stream stream = dimImageFilter.GetDataForkStream();

            stream.Seek((long)(DATA_OFFSET + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }

        public virtual byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public virtual bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public virtual bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual bool? VerifyMediaImage()
        {
            return null;
        }

        enum DiskType : byte
        {
            Hd2 = 0,
            Hs2 = 1,
            Hc2 = 2,
            Hde2 = 3,
            Hq2 = 9,
            N88 = 17
        }
    }
}