// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ZZZRawImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages raw image, that is, user data sector by sector copy.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.ImagePlugins
{
    // Checked using several images and strings inside Apple's DiskImages.framework
    class ZZZRawImage : ImagePlugin
    {
        #region Internal variables

        string rawImagePath;
        bool differentTrackZeroSize;

        #endregion

        public ZZZRawImage()
        {
            Name = "Raw Disk Image";
            // Non-random UUID to recognize this specific plugin
            PluginUUID = new Guid("12345678-AAAA-BBBB-CCCC-123456789000");
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
        }

        public override bool IdentifyImage(string imagePath)
        {
            FileInfo fi = new FileInfo(imagePath);

            // Check if file is not multiple of 512
            if((fi.Length % 512) != 0)
            {
                // Check known disk sizes with sectors smaller than 512
                switch(fi.Length)
                {
                    case 81664:
                    case 116480:
                    case 242944:
                    case 256256:
                    case 287488:
                    case 306432:
                    case 495872:
                    case 988416:
                    case 995072:
                    case 1021696:
                    case 1146624:
                    case 1177344:
                    case 1222400:
                    case 1304320:
                    case 1255168:
                        return true;
                    default:
                        return false;
                }
            }

            return true;
        }

        public override bool OpenImage(string imagePath)
        {
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Close();

            FileInfo fi = new FileInfo(imagePath);
            string extension = Path.GetExtension(imagePath).ToLower();
            if(extension == ".iso" && (fi.Length % 2048) == 0)
                ImageInfo.sectorSize = 2048;
            else
            {
                switch(fi.Length)
                {
                    case 242944:
                    case 256256:
                    case 495872:
                    case 92160:
                    case 133120:
                        ImageInfo.sectorSize = 128;
                        break;
                    case 116480:
                    case 287488: // T0S0 = 128bps
                    case 988416: // T0S0 = 128bps
                    case 995072: // T0S0 = 128bps, T0S1 = 256bps
                    case 1021696: // T0S0 = 128bps, T0S1 = 256bps
                    case 232960:
                    case 143360:
                    case 286720:
                    case 512512:
                    case 102400:
                    case 204800:
                    case 163840:
                    case 327680:
                    case 655360:
                    case 80384: // T0S0 = 128bps
                    case 325632: // T0S0 = 128bps, T0S1 = 256bps
                    case 653312: // T0S0 = 128bps, T0S1 = 256bps
                        ImageInfo.sectorSize = 256;
                        break;
                    case 81664:
                        ImageInfo.sectorSize = 319;
                        break;
                    case 306432: // T0S0 = 128bps
                    case 1146624: // T0S0 = 128bps, T0S1 = 256bps
                    case 1177344: // T0S0 = 128bps, T0S1 = 256bps
                        ImageInfo.sectorSize = 512;
                        break;
                    case 1222400: // T0S0 = 128bps, T0S1 = 256bps
                    case 1304320: // T0S0 = 128bps, T0S1 = 256bps
                    case 1255168: // T0S0 = 128bps, T0S1 = 256bps
                    case 1261568:
                    case 1310720:
                        ImageInfo.sectorSize = 1024;
                        break;
                    default:
                        ImageInfo.sectorSize = 512;
                        break;
                }
            }

            ImageInfo.imageSize = (ulong)fi.Length;
            ImageInfo.imageCreationTime = fi.CreationTimeUtc;
            ImageInfo.imageLastModificationTime = fi.LastWriteTimeUtc;
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imagePath);
            differentTrackZeroSize = false;
            rawImagePath = imagePath;

            switch(fi.Length)
            {
                case 242944:
                    ImageInfo.sectors = 1898;
                    break;
                case 256256:
                    ImageInfo.sectors = 2002;
                    break;
                case 495872:
                    ImageInfo.sectors = 3874;
                    break;
                case 116480:
                    ImageInfo.sectors = 455;
                    break;
                case 287488: // T0S0 = 128bps
                    ImageInfo.sectors = 1136;
                    differentTrackZeroSize = true;
                    break;
                case 988416: // T0S0 = 128bps
                    ImageInfo.sectors = 3874;
                    differentTrackZeroSize = true;
                    break;
                case 995072: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.sectors = 3900;
                    differentTrackZeroSize = true;
                    break;
                case 1021696: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.sectors = 4004;
                    differentTrackZeroSize = true;
                    break;
                case 81664:
                    ImageInfo.sectors = 256;
                    break;
                case 306432: // T0S0 = 128bps
                    ImageInfo.sectors = 618;
                    differentTrackZeroSize = true;
                    break;
                case 1146624: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.sectors = 2272;
                    differentTrackZeroSize = true;
                    break;
                case 1177344: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.sectors = 2332;
                    differentTrackZeroSize = true;
                    break;
                case 1222400: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.sectors = 1236;
                    differentTrackZeroSize = true;
                    break;
                case 1304320: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.sectors = 1316;
                    differentTrackZeroSize = true;
                    break;
                case 1255168: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.sectors = 1268;
                    differentTrackZeroSize = true;
                    break;
                case 80384: // T0S0 = 128bps
                    ImageInfo.sectors = 322;
                    differentTrackZeroSize = true;
                    break;
                case 325632: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.sectors = 1280;
                    differentTrackZeroSize = true;
                    break;
                case 653312: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.sectors = 2560;
                    differentTrackZeroSize = true;
                    break;
                case 1880064: // IBM XDF, 3,5", real number of sectors
                    ImageInfo.sectors = 670;
                    ImageInfo.sectorSize = 8192; // Biggest sector size
                    differentTrackZeroSize = true;
                    break;
                default:
                    ImageInfo.sectors = ImageInfo.imageSize / ImageInfo.sectorSize;
                    break;
            }

            ImageInfo.mediaType = CalculateDiskType();

            switch(ImageInfo.mediaType)
            {
                case MediaType.CD:
                case MediaType.DVDPR:
                case MediaType.DVDR:
                case MediaType.DVDRDL:
                case MediaType.DVDPRDL:
                case MediaType.BDR:
                case MediaType.BDRXL:
                    ImageInfo.xmlMediaType = XmlMediaType.OpticalDisc;
                    break;
                default:
                    ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
                    break;
            }

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.imageHasPartitions;
        }

        public override UInt64 GetImageSize()
        {
            return ImageInfo.imageSize;
        }

        public override UInt64 GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override UInt32 GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override byte[] ReadSector(UInt64 sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(UInt64 sectorAddress, UInt32 length)
        {
            if(differentTrackZeroSize)
            {
                throw new NotImplementedException("Not yet implemented");
            }
            else
            {
                if(sectorAddress > ImageInfo.sectors - 1)
                    throw new ArgumentOutOfRangeException("sectorAddress", "Sector address not found");

                if(sectorAddress + length > ImageInfo.sectors)
                    throw new ArgumentOutOfRangeException("length", "Requested more sectors than available");

                byte[] buffer = new byte[length * ImageInfo.sectorSize];

                FileStream stream = new FileStream(rawImagePath, FileMode.Open, FileAccess.Read);

                stream.Seek((long)(sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);

                stream.Read(buffer, 0, (int)(length * ImageInfo.sectorSize));

                stream.Close();

                return buffer;

            }
        }

        public override string GetImageFormat()
        {
            return "Raw disk image (sector by sector copy)";
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

        public override bool? VerifySector(UInt64 sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(UInt64 sectorAddress, UInt32 track)
        {
            return null;
        }

        public override bool? VerifySectors(UInt64 sectorAddress, UInt32 length, out List<UInt64> FailingLBAs, out List<UInt64> UnknownLBAs)
        {
            FailingLBAs = new List<UInt64>();
            UnknownLBAs = new List<UInt64>();

            for(UInt64 i = sectorAddress; i < sectorAddress + length; i++)
                UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifySectors(UInt64 sectorAddress, UInt32 length, UInt32 track, out List<UInt64> FailingLBAs, out List<UInt64> UnknownLBAs)
        {
            FailingLBAs = new List<UInt64>();
            UnknownLBAs = new List<UInt64>();

            for(UInt64 i = sectorAddress; i < sectorAddress + length; i++)
                UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }

        public override List<Track> GetTracks()
        {
            if(ImageInfo.xmlMediaType == XmlMediaType.OpticalDisc)
            {
                Track trk = new Track();
                trk.TrackBytesPerSector = (int)ImageInfo.sectorSize;
                trk.TrackEndSector = ImageInfo.sectors - 1;
                trk.TrackFile = rawImagePath;
                trk.TrackFileOffset = 0;
                trk.TrackFileType = "BINARY";
                trk.TrackRawBytesPerSector = (int)ImageInfo.sectorSize;
                trk.TrackSequence = 1;
                trk.TrackStartSector = 0;
                trk.TrackSubchannelType = TrackSubchannelType.None;
                trk.TrackType = TrackType.Data;
                trk.TrackSession = 1;
                List<Track> lst = new List<Track>();
                lst.Add(trk);
                return lst;
            }

            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            if(ImageInfo.xmlMediaType == XmlMediaType.OpticalDisc)
            {
                if(session.SessionSequence != 1)
                    throw new ArgumentOutOfRangeException("session", "Only a single session is supported");

                Track trk = new Track();
                trk.TrackBytesPerSector = (int)ImageInfo.sectorSize;
                trk.TrackEndSector = ImageInfo.sectors - 1;
                trk.TrackFile = rawImagePath;
                trk.TrackFileOffset = 0;
                trk.TrackFileType = "BINARY";
                trk.TrackRawBytesPerSector = (int)ImageInfo.sectorSize;
                trk.TrackSequence = 1;
                trk.TrackStartSector = 0;
                trk.TrackSubchannelType = TrackSubchannelType.None;
                trk.TrackType = TrackType.Data;
                trk.TrackSession = 1;
                List<Track> lst = new List<Track>();
                lst.Add(trk);
                return lst;
            }

            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(UInt16 session)
        {
            if(ImageInfo.xmlMediaType == XmlMediaType.OpticalDisc)
            {
                if(session != 1)
                    throw new ArgumentOutOfRangeException("session", "Only a single session is supported");

                Track trk = new Track();
                trk.TrackBytesPerSector = (int)ImageInfo.sectorSize;
                trk.TrackEndSector = ImageInfo.sectors - 1;
                trk.TrackFile = rawImagePath;
                trk.TrackFileOffset = 0;
                trk.TrackFileType = "BINARY";
                trk.TrackRawBytesPerSector = (int)ImageInfo.sectorSize;
                trk.TrackSequence = 1;
                trk.TrackStartSector = 0;
                trk.TrackSubchannelType = TrackSubchannelType.None;
                trk.TrackType = TrackType.Data;
                trk.TrackSession = 1;
                List<Track> lst = new List<Track>();
                lst.Add(trk);
                return lst;
            }

            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Session> GetSessions()
        {
            if(ImageInfo.xmlMediaType == XmlMediaType.OpticalDisc)
            {
                Session sess = new Session();
                sess.EndSector = ImageInfo.sectors - 1;
                sess.EndTrack = 1;
                sess.SessionSequence = 1;
                sess.StartSector = 0;
                sess.StartTrack = 1;
                List<Session> lst = new List<Session>();
                lst.Add(sess);
                return lst;
            }

            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSector(UInt64 sectorAddress, UInt32 track)
        {
            if(ImageInfo.xmlMediaType == XmlMediaType.OpticalDisc)
            {
                if(track != 1)
                    throw new ArgumentOutOfRangeException("track", "Only a single session is supported");

                return ReadSector(sectorAddress);
            }

            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(UInt64 sectorAddress, UInt32 length, UInt32 track)
        {
            if(ImageInfo.xmlMediaType == XmlMediaType.OpticalDisc)
            {
                if(track != 1)
                    throw new ArgumentOutOfRangeException("track", "Only a single session is supported");

                return ReadSectors(sectorAddress, length);
            }

            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(UInt64 sectorAddress, UInt32 track)
        {
            if(ImageInfo.xmlMediaType == XmlMediaType.OpticalDisc)
            {
                if(track != 1)
                    throw new ArgumentOutOfRangeException("track", "Only a single session is supported");

                return ReadSector(sectorAddress);
            }

            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length, UInt32 track)
        {
            if(ImageInfo.xmlMediaType == XmlMediaType.OpticalDisc)
            {
                if(track != 1)
                    throw new ArgumentOutOfRangeException("track", "Only a single session is supported");

                return ReadSectors(sectorAddress, length);
            }

            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        #region Private methods

        MediaType CalculateDiskType()
        {
            if(ImageInfo.sectorSize == 2048)
            {
                if(ImageInfo.sectors <= 360000)
                    return MediaType.CD;
                if(ImageInfo.sectors <= 2295104)
                    return MediaType.DVDPR;
                if(ImageInfo.sectors <= 2298496)
                    return MediaType.DVDR;
                if(ImageInfo.sectors <= 4171712)
                    return MediaType.DVDRDL;
                if(ImageInfo.sectors <= 4173824)
                    return MediaType.DVDPRDL;
                if(ImageInfo.sectors <= 24438784)
                    return MediaType.BDR;
                if(ImageInfo.sectors <= 62500864)
                    return MediaType.BDRXL;
                return MediaType.Unknown;
            }
            else
            {
                switch(ImageInfo.imageSize)
                {
                    case 80384:
                        return MediaType.ECMA_66;
                    case 81664:
                        return MediaType.IBM23FD;
                    case 92160:
                        return MediaType.ATARI_525_SD;
                    case 102400:
                        return MediaType.ACORN_525_SS_SD_40;
                    case 116480:
                        return MediaType.Apple32SS;
                    case 133120:
                        return MediaType.ATARI_525_ED;
                    case 143360:
                        return MediaType.Apple33SS;
                    case 163840:
                        return MediaType.DOS_525_SS_DD_8;
                    case 184320:
                        return MediaType.DOS_525_SS_DD_9;
                    case 204800:
                        return MediaType.ACORN_525_SS_SD_80;
                    case 232960:
                        return MediaType.Apple32DS;
                    case 242944:
                        return MediaType.IBM33FD_128;
                    case 256256:
                        return MediaType.ECMA_54;
                    case 286720:
                        return MediaType.Apple33DS;
                    case 287488:
                        return MediaType.IBM33FD_256;
                    case 306432:
                        return MediaType.IBM33FD_512;
                    case 325632:
                        return MediaType.ECMA_70;
                    case 327680:
                        return MediaType.DOS_525_DS_DD_8;
                    case 368640:
                        return MediaType.DOS_525_DS_DD_9;
                    case 409600:
                        return MediaType.AppleSonySS;
                    case 495872:
                        return MediaType.IBM43FD_128;
                    case 512512:
                        return MediaType.ECMA_59;
                    case 653312:
                        return MediaType.ECMA_78;
                    case 655360:
                        return MediaType.ACORN_525_DS_DD;
                    case 737280:
                        return MediaType.DOS_35_DS_DD_9;
                    case 819200:
                        return MediaType.AppleSonyDS;
                    case 839680:
                        return MediaType.FDFORMAT_35_DD;
                    case 901120:
                        return MediaType.CBM_AMIGA_35_DD;
                    case 988416:
                        return MediaType.IBM43FD_256;
                    case 995072:
                        return MediaType.IBM53FD_256;
                    case 1021696:
                        return MediaType.ECMA_99_26;
                    case 1146624:
                        return MediaType.IBM53FD_512;
                    case 1177344:
                        return MediaType.ECMA_99_15;
                    case 1222400:
                        return MediaType.IBM53FD_1024;
                    case 1228800:
                        return MediaType.DOS_525_HD;
                    case 1255168:
                        return MediaType.ECMA_69_8;
                    case 1261568:
                        return MediaType.NEC_8_DD;
                    case 1304320:
                        return MediaType.ECMA_99_8;
                    case 1310720:
                        return MediaType.NEC_525_HD;
                    case 1427456:
                        return MediaType.FDFORMAT_525_HD;
                    case 1474560:
                        return MediaType.DOS_35_HD;
                    case 1720320:
                        return MediaType.DMF;
                    case 1763328:
                        return MediaType.FDFORMAT_35_HD;
                    case 1802240:
                        return MediaType.CBM_AMIGA_35_HD;
                    case 1880064:
                        return MediaType.XDF_35;
                    case 1884160:
                        return MediaType.XDF_35;
                    case 2949120:
                        return MediaType.DOS_35_ED;
                    case 128000000:
                        return MediaType.ECMA_154;
                    case 229632000:
                        return MediaType.ECMA_201;
                    case 481520640:
                        return MediaType.ECMA_183_512;
                    case 533403648:
                        return MediaType.ECMA_183;
                    case 596787200:
                        return MediaType.ECMA_184_512;
                    case 654540800:
                        return MediaType.ECMA_184;
                    default:
                        return MediaType.GENERIC_HDD;
                }
            }
        }

        #endregion

        #region Unsupported features

        public override byte[] ReadSectorTag(UInt64 sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(UInt64 sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
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

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
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

        public override List<CommonTypes.Partition> GetPartitions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(UInt64 sectorAddress, UInt32 track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, UInt32 track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        #endregion Unsupported features
    }
}

