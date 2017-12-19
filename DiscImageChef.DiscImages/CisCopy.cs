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
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
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
    public class CisCopy : ImagePlugin
    {
        #region Internal enumerations
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
        #endregion Internal enumeration

        #region Internal variables
        byte[] decodedDisk;
        #endregion Internal variables

        public CisCopy()
        {
            Name = "CisCopy Disk Image (DC-File)";
            PluginUUID = new Guid("EDF20CC7-6012-49E2-9E92-663A53E42130");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplication = null;
            ImageInfo.imageApplicationVersion = null;
            ImageInfo.imageCreator = null;
            ImageInfo.imageComments = null;
            ImageInfo.mediaManufacturer = null;
            ImageInfo.mediaModel = null;
            ImageInfo.mediaSerialNumber = null;
            ImageInfo.mediaBarcode = null;
            ImageInfo.mediaPartNumber = null;
            ImageInfo.mediaSequence = 0;
            ImageInfo.lastMediaSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
            ImageInfo.driveFirmwareRevision = null;
        }

        #region Public methods
        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            DiskType type = (DiskType)stream.ReadByte();
            byte tracks = 0;

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
            {
                if(trackBytes[i] != (byte)TrackType.Copied && trackBytes[i] != (byte)TrackType.Omitted &&
                   trackBytes[i] != (byte)TrackType.OmittedAlternate) return false;
            }

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

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            MemoryStream decodedImage;

            DiskType type = (DiskType)stream.ReadByte();
            byte tracks = 0;

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
                default: throw new ImageNotSupportedException(string.Format("Incorrect disk type {0}", (byte)type));
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

            decodedImage = new MemoryStream();

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

            ImageInfo.imageApplication = "CisCopy";
            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = imageFilter.GetFilename();
            ImageInfo.imageSize = (ulong)(stream.Length - 2 - trackBytes.Length);
            ImageInfo.sectorSize = 512;

            switch(type)
            {
                case DiskType.MD1DD8:
                    ImageInfo.mediaType = MediaType.DOS_525_SS_DD_8;
                    ImageInfo.sectors = 40 * 1 * 8;
                    ImageInfo.heads = 1;
                    ImageInfo.cylinders = 40;
                    ImageInfo.sectorsPerTrack = 8;
                    break;
                case DiskType.MD2DD8:
                    ImageInfo.mediaType = MediaType.DOS_525_DS_DD_8;
                    ImageInfo.sectors = 40 * 2 * 8;
                    ImageInfo.heads = 2;
                    ImageInfo.cylinders = 40;
                    ImageInfo.sectorsPerTrack = 8;
                    break;
                case DiskType.MD1DD:
                    ImageInfo.mediaType = MediaType.DOS_525_SS_DD_9;
                    ImageInfo.sectors = 40 * 1 * 9;
                    ImageInfo.heads = 1;
                    ImageInfo.cylinders = 40;
                    ImageInfo.sectorsPerTrack = 9;
                    break;
                case DiskType.MD2DD:
                    ImageInfo.mediaType = MediaType.DOS_525_DS_DD_9;
                    ImageInfo.sectors = 40 * 2 * 9;
                    ImageInfo.heads = 2;
                    ImageInfo.cylinders = 40;
                    ImageInfo.sectorsPerTrack = 9;
                    break;
                case DiskType.MF2DD:
                    ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;
                    ImageInfo.sectors = 80 * 2 * 9;
                    ImageInfo.heads = 2;
                    ImageInfo.cylinders = 80;
                    ImageInfo.sectorsPerTrack = 9;
                    break;
                case DiskType.MD2HD:
                    ImageInfo.mediaType = MediaType.DOS_525_HD;
                    ImageInfo.sectors = 80 * 2 * 15;
                    ImageInfo.heads = 2;
                    ImageInfo.cylinders = 80;
                    ImageInfo.sectorsPerTrack = 15;
                    break;
                case DiskType.MF2HD:
                    ImageInfo.mediaType = MediaType.DOS_35_HD;
                    ImageInfo.sectors = 80 * 2 * 18;
                    ImageInfo.heads = 2;
                    ImageInfo.cylinders = 80;
                    ImageInfo.sectorsPerTrack = 18;
                    break;
            }

            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
            decodedDisk = decodedImage.ToArray();

            decodedImage.Close();

            DicConsole.VerboseWriteLine("CisCopy image contains a disk of type {0}", ImageInfo.mediaType);

            return true;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs,
                                            out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs,
                                            out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.imageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.imageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.sectorSize];

            Array.Copy(decodedDisk, (int)sectorAddress * ImageInfo.sectorSize, buffer, 0,
                       length * ImageInfo.sectorSize);

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "CisCopy";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.imageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.imageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.mediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.mediaModel;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.mediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.mediaBarcode;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.mediaPartNumber;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.mediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.lastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.driveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.driveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.driveSerialNumber;
        }
        #endregion Public methods

        #region Unsupported features
        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Partition> GetPartitions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetTracks()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Session> GetSessions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        #endregion Unsupported features
    }
}