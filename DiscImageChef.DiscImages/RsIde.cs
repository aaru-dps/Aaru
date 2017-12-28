// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RsIde.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages RS-IDE disk images.
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
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filters;
using static DiscImageChef.Decoders.ATA.Identify;

namespace DiscImageChef.DiscImages
{
    public class RsIde : IMediaImage
    {
        readonly byte[] signature = {0x52, 0x53, 0x2D, 0x49, 0x44, 0x45, 0x1A};
        ushort dataOff;
        byte[] identify;
        ImageInfo imageInfo;

        IFilter rsIdeImageFilter;

        public RsIde()
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

        public string Name => "RS-IDE Hard Disk Image";
        public Guid Id => new Guid("47C3E78D-2BE2-4BA5-AA6B-FEE27C86FC65");
        public ImageInfo Info => imageInfo;

        public string Format => "RS-IDE disk image";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] magic = new byte[7];
            stream.Read(magic, 0, magic.Length);

            return magic.SequenceEqual(signature);
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[Marshal.SizeOf(typeof(RsIdeHeader))];
            stream.Read(hdrB, 0, hdrB.Length);

            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RsIdeHeader)));
            Marshal.Copy(hdrB, 0, hdrPtr, Marshal.SizeOf(typeof(RsIdeHeader)));
            RsIdeHeader hdr = (RsIdeHeader)Marshal.PtrToStructure(hdrPtr, typeof(RsIdeHeader));
            Marshal.FreeHGlobal(hdrPtr);

            if(!hdr.magic.SequenceEqual(signature)) return false;

            dataOff = hdr.dataOff;

            imageInfo.MediaType = MediaType.GENERIC_HDD;
            imageInfo.SectorSize = (uint)(hdr.flags.HasFlag(RsIdeFlags.HalfSectors) ? 256 : 512);
            imageInfo.ImageSize = (ulong)(stream.Length - dataOff);
            imageInfo.Sectors = imageInfo.ImageSize / imageInfo.SectorSize;
            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            imageInfo.Version = $"{hdr.revision >> 8}.{hdr.revision & 0x0F}";

            if(!ArrayHelpers.ArrayIsNullOrEmpty(hdr.identify))
            {
                identify = new byte[512];
                Array.Copy(hdr.identify, 0, identify, 0, hdr.identify.Length);
                IdentifyDevice? ataId = Decode(identify);

                if(ataId.HasValue)
                {
                    imageInfo.ReadableMediaTags.Add(MediaTagType.ATA_IDENTIFY);
                    imageInfo.Cylinders = ataId.Value.Cylinders;
                    imageInfo.Heads = ataId.Value.Heads;
                    imageInfo.SectorsPerTrack = ataId.Value.SectorsPerCard;
                    imageInfo.DriveFirmwareRevision = ataId.Value.FirmwareRevision;
                    imageInfo.DriveModel = ataId.Value.Model;
                    imageInfo.DriveSerialNumber = ataId.Value.SerialNumber;
                    imageInfo.MediaSerialNumber = ataId.Value.MediaSerial;
                    imageInfo.MediaManufacturer = ataId.Value.MediaManufacturer;
                }
            }

            if(imageInfo.Cylinders == 0 || imageInfo.Heads == 0 || imageInfo.SectorsPerTrack == 0)
            {
                imageInfo.Cylinders = (uint)(imageInfo.Sectors / 16 / 63);
                imageInfo.Heads = 16;
                imageInfo.SectorsPerTrack = 63;
            }

            rsIdeImageFilter = imageFilter;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Stream stream = rsIdeImageFilter.GetDataForkStream();

            stream.Seek((long)(dataOff + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(!imageInfo.ReadableMediaTags.Contains(tag) || tag != MediaTagType.ATA_IDENTIFY)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            byte[] buffer = new byte[512];
            Array.Copy(identify, 0, buffer, 0, 512);
            return buffer;
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
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

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
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

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RsIdeHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)] public byte[] magic;
            public byte revision;
            public RsIdeFlags flags;
            public ushort dataOff;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public byte[] reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 106)] public byte[] identify;
        }

        [Flags]
        enum RsIdeFlags : byte
        {
            HalfSectors = 1
        }
    }
}