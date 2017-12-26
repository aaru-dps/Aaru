// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CisCopy.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages CisCopy disk images.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    /* This is a very simple format created by a German application called CisCopy, aka CCOPY.EXE, with extension .DCF.
     * First byte indicates the floppy type, limited to standard formats.
     * Indeed if the floppy is not DOS formatted, user must choose from the list of supported formats manually.
     * Next 80 bytes (for 5.25" DD disks) or 160 bytes (for 5.25" HD and 3.5" disks) indicate if a track has been copied
     * or not.
     * It offers three copy methods:
     * a) All, copies all tracks
     * b) FAT, copies all tracks which contain sectors marked as sued by FAT
     * c) "Belelung" similarly to FAT. On some disk tests FAT cuts data, while belelung does not.
     * Finally, next byte indicates compression:
     * 0) No compression
     * 1) Normal compression, algorithm unknown
     * 2) High compression, algorithm unknown
     * Then the data for whole tracks follow.
     */
    public class CisCopy : IMediaImage
    {
        byte[] decodedDisk;
        ImageInfo imageInfo;

        public CisCopy()
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

        public virtual string Name => "CisCopy Disk Image (DC-File)";
        public virtual Guid Id => new Guid("EDF20CC7-6012-49E2-9E92-663A53E42130");

        public virtual string ImageFormat => "CisCopy";

        public virtual List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public virtual List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public virtual List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        public virtual ImageInfo Info => imageInfo;

        public virtual bool IdentifyImage(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            DiskType type = (DiskType)stream.ReadByte();
            byte tracks;

            switch(type)
            {
                case DiskType.MD1DD8:
                case DiskType.MD1DD:
                case DiskType.MD2DD8:
                case DiskType.MD2DD:
                    tracks = 80;
                    break;
                case DiskType.MF2DD:
                case DiskType.MD2HD:
                case DiskType.MF2HD:
                    tracks = 160;
                    break;
                default: return false;
            }

            byte[] trackBytes = new byte[tracks];
            stream.Read(trackBytes, 0, tracks);

            for(int i = 0; i < tracks; i++)
                if(trackBytes[i] != (byte)TrackType.Copied && trackBytes[i] != (byte)TrackType.Omitted &&
                   trackBytes[i] != (byte)TrackType.OmittedAlternate) return false;

            Compression cmpr = (Compression)stream.ReadByte();

            if(cmpr != Compression.None && cmpr != Compression.Normal && cmpr != Compression.High) return false;

            switch(type)
            {
                case DiskType.MD1DD8:
                    if(stream.Length > 40 * 1 * 8 * 512 + 82) return false;

                    break;
                case DiskType.MD1DD:
                    if(stream.Length > 40 * 1 * 9 * 512 + 82) return false;

                    break;
                case DiskType.MD2DD8:
                    if(stream.Length > 40 * 2 * 8 * 512 + 82) return false;

                    break;
                case DiskType.MD2DD:
                    if(stream.Length > 40 * 2 * 9 * 512 + 82) return false;

                    break;
                case DiskType.MF2DD:
                    if(stream.Length > 80 * 2 * 9 * 512 + 162) return false;

                    break;
                case DiskType.MD2HD:
                    if(stream.Length > 80 * 2 * 15 * 512 + 162) return false;

                    break;
                case DiskType.MF2HD:
                    if(stream.Length > 80 * 2 * 18 * 512 + 162) return false;

                    break;
            }

            return true;
        }

        public virtual bool OpenImage(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            DiskType type = (DiskType)stream.ReadByte();
            byte tracks;

            switch(type)
            {
                case DiskType.MD1DD8:
                case DiskType.MD1DD:
                case DiskType.MD2DD8:
                case DiskType.MD2DD:
                    tracks = 80;
                    break;
                case DiskType.MF2DD:
                case DiskType.MD2HD:
                case DiskType.MF2HD:
                    tracks = 160;
                    break;
                default: throw new ImageNotSupportedException($"Incorrect disk type {(byte)type}");
            }

            byte[] trackBytes = new byte[tracks];
            stream.Read(trackBytes, 0, tracks);

            Compression cmpr = (Compression)stream.ReadByte();

            if(cmpr != Compression.None)
                throw new FeatureSupportedButNotImplementedImageException("Compressed images are not supported.");

            int tracksize = 0;

            switch(type)
            {
                case DiskType.MD1DD8:
                case DiskType.MD2DD8:
                    tracksize = 8 * 512;
                    break;
                case DiskType.MD1DD:
                case DiskType.MD2DD:
                case DiskType.MF2DD:
                    tracksize = 9 * 512;
                    break;
                case DiskType.MD2HD:
                    tracksize = 15 * 512;
                    break;
                case DiskType.MF2HD:
                    tracksize = 18 * 512;
                    break;
            }

            int headstep = 1;
            if(type == DiskType.MD1DD || type == DiskType.MD1DD8) headstep = 2;

            MemoryStream decodedImage = new MemoryStream();

            for(int i = 0; i < tracks; i += headstep)
            {
                byte[] track = new byte[tracksize];

                if((TrackType)trackBytes[i] == TrackType.Copied) stream.Read(track, 0, tracksize);
                else ArrayHelpers.ArrayFill(track, (byte)0xF6);

                decodedImage.Write(track, 0, tracksize);
            }

            /*
                        FileStream debugStream = new FileStream("debug.img", FileMode.CreateNew, FileAccess.ReadWrite);
                        debugStream.Write(decodedImage.ToArray(), 0, (int)decodedImage.Length);
                        debugStream.Close();
            */

            imageInfo.Application = "CisCopy";
            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle = imageFilter.GetFilename();
            imageInfo.ImageSize = (ulong)(stream.Length - 2 - trackBytes.Length);
            imageInfo.SectorSize = 512;

            switch(type)
            {
                case DiskType.MD1DD8:
                    imageInfo.MediaType = MediaType.DOS_525_SS_DD_8;
                    imageInfo.Sectors = 40 * 1 * 8;
                    imageInfo.Heads = 1;
                    imageInfo.Cylinders = 40;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case DiskType.MD2DD8:
                    imageInfo.MediaType = MediaType.DOS_525_DS_DD_8;
                    imageInfo.Sectors = 40 * 2 * 8;
                    imageInfo.Heads = 2;
                    imageInfo.Cylinders = 40;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case DiskType.MD1DD:
                    imageInfo.MediaType = MediaType.DOS_525_SS_DD_9;
                    imageInfo.Sectors = 40 * 1 * 9;
                    imageInfo.Heads = 1;
                    imageInfo.Cylinders = 40;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case DiskType.MD2DD:
                    imageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
                    imageInfo.Sectors = 40 * 2 * 9;
                    imageInfo.Heads = 2;
                    imageInfo.Cylinders = 40;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case DiskType.MF2DD:
                    imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                    imageInfo.Sectors = 80 * 2 * 9;
                    imageInfo.Heads = 2;
                    imageInfo.Cylinders = 80;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case DiskType.MD2HD:
                    imageInfo.MediaType = MediaType.DOS_525_HD;
                    imageInfo.Sectors = 80 * 2 * 15;
                    imageInfo.Heads = 2;
                    imageInfo.Cylinders = 80;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case DiskType.MF2HD:
                    imageInfo.MediaType = MediaType.DOS_35_HD;
                    imageInfo.Sectors = 80 * 2 * 18;
                    imageInfo.Heads = 2;
                    imageInfo.Cylinders = 80;
                    imageInfo.SectorsPerTrack = 18;
                    break;
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            decodedDisk = decodedImage.ToArray();

            decodedImage.Close();

            DicConsole.VerboseWriteLine("CisCopy image contains a disk of type {0}", imageInfo.MediaType);

            return true;
        }

        public virtual bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public virtual bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public virtual bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public virtual bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public virtual bool? VerifyMediaImage()
        {
            return null;
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

            Array.Copy(decodedDisk, (int)sectorAddress * imageInfo.SectorSize, buffer, 0,
                       length * imageInfo.SectorSize);

            return buffer;
        }

        public virtual byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadDiskTag(MediaTagType tag)
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

        public virtual byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
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

        public virtual byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        enum DiskType : byte
        {
            MD1DD8 = 1,
            MD1DD = 2,
            MD2DD8 = 3,
            MD2DD = 4,
            MF2DD = 5,
            MD2HD = 6,
            MF2HD = 7
        }

        enum Compression : byte
        {
            None = 0,
            Normal = 1,
            High = 2
        }

        enum TrackType : byte
        {
            Copied = 0x4C,
            Omitted = 0xFA,
            OmittedAlternate = 0xFE
        }
    }
}