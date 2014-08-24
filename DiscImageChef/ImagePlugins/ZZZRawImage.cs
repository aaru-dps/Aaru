/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : ZZZRawImage.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Disc image plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Manages raw image, that is, user data sector by sector copy.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.IO;
using System.Collections.Generic;

namespace DiscImageChef.ImagePlugins
{
    // Checked using several images and strings inside Apple's DiskImages.framework
    class ZZZRawImage : ImagePlugin
    {
        #region Internal variables
        string rawImagePath;
        bool differentTrackZeroSize;
        #endregion

        #region Accesible variables

        ImageInfo _imageInfo;

        public ImageInfo ImageInfo
        {
            get
            {
                return _imageInfo;
            }
        }

        #endregion

        public ZZZRawImage(PluginBase Core)
        {
            Name = "Raw Disk Image";
            // Non-random UUID to recognize this specific plugin
            PluginUUID = new Guid("12345678-AAAA-BBBB-CCCC-123456789000");
            _imageInfo = new ImageInfo();
            _imageInfo.readableSectorTags = new List<SectorTagType>();
            _imageInfo.readableDiskTags = new List<DiskTagType>();
            _imageInfo.imageHasPartitions = false;
            _imageInfo.imageHasSessions = false;
            _imageInfo.imageVersion = null;
            _imageInfo.imageApplication = null;
            _imageInfo.imageApplicationVersion = null;
            _imageInfo.imageCreator = null;
            _imageInfo.imageComments = null;
            _imageInfo.diskManufacturer = null;
            _imageInfo.diskModel = null;
            _imageInfo.diskSerialNumber = null;
            _imageInfo.diskBarcode = null;
            _imageInfo.diskPartNumber = null;
            _imageInfo.diskSequence = 0;
            _imageInfo.lastDiskSequence = 0;
            _imageInfo.driveManufacturer = null;
            _imageInfo.driveModel = null;
            _imageInfo.driveSerialNumber = null;
        }

        public override bool IdentifyImage(string imagePath)
        {
            FileInfo fi = new FileInfo(imagePath);

            // Check if file is not multiple of 512
            if ((fi.Length % 512) != 0)
            {
                // Check known disk sizes with sectors smaller than 512
                switch (fi.Length)
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
            if (extension == ".iso" && (fi.Length % 2048) == 0)
                _imageInfo.sectorSize = 2048;
            else
            {
                switch (fi.Length)
                {
                    case 242944:
                    case 256256:
                    case 495872:
                    case 92160:
                    case 133120:
                        _imageInfo.sectorSize = 128;
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
                        _imageInfo.sectorSize = 256;
                        break;
                    case 81664:
                        _imageInfo.sectorSize = 319;
                        break;
                    case 306432: // T0S0 = 128bps
                    case 1146624: // T0S0 = 128bps, T0S1 = 256bps
                    case 1177344: // T0S0 = 128bps, T0S1 = 256bps
                        _imageInfo.sectorSize = 512;
                        break;
                    case 1222400: // T0S0 = 128bps, T0S1 = 256bps
                    case 1304320: // T0S0 = 128bps, T0S1 = 256bps
                    case 1255168: // T0S0 = 128bps, T0S1 = 256bps
                    case 1261568:
                    case 1310720:
                        _imageInfo.sectorSize = 1024;
                        break;
                    default:
                        _imageInfo.sectorSize = 512;
                        break;
                }
            }

            _imageInfo.imageSize = (ulong)fi.Length;
            _imageInfo.imageCreationTime = fi.CreationTimeUtc;
            _imageInfo.imageLastModificationTime = fi.LastWriteTimeUtc;
            _imageInfo.imageName = Path.GetFileNameWithoutExtension(imagePath);
            differentTrackZeroSize = false;
            rawImagePath = imagePath;

            switch (fi.Length)
            {
                case 242944:
                    _imageInfo.sectors = 1898;
                    break;
                case 256256:
                    _imageInfo.sectors = 2002;
                    break;
                case 495872:
                    _imageInfo.sectors = 3874;
                    break;
                case 116480:
                    _imageInfo.sectors = 455;
                    break;
                case 287488: // T0S0 = 128bps
                    _imageInfo.sectors = 1136;
                    differentTrackZeroSize = true;
                    break;
                case 988416: // T0S0 = 128bps
                    _imageInfo.sectors = 3874;
                    differentTrackZeroSize = true;
                    break;
                case 995072: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.sectors = 3900;
                    differentTrackZeroSize = true;
                    break;
                case 1021696: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.sectors = 4004;
                    differentTrackZeroSize = true;
                    break;
                case 81664:
                    _imageInfo.sectors = 256;
                    break;
                case 306432: // T0S0 = 128bps
                    _imageInfo.sectors = 618;
                    differentTrackZeroSize = true;
                    break;
                case 1146624: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.sectors = 2272;
                    differentTrackZeroSize = true;
                    break;
                case 1177344: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.sectors = 2332;
                    differentTrackZeroSize = true;
                    break;
                case 1222400: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.sectors = 1236;
                    differentTrackZeroSize = true;
                    break;
                case 1304320: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.sectors = 1316;
                    differentTrackZeroSize = true;
                    break;
                case 1255168: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.sectors = 1268;
                    differentTrackZeroSize = true;
                    break;
                case 80384: // T0S0 = 128bps
                    _imageInfo.sectors = 322;
                    differentTrackZeroSize = true;
                    break;
                case 325632: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.sectors = 1280;
                    differentTrackZeroSize = true;
                    break;
                case 653312: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.sectors = 2560;
                    differentTrackZeroSize = true;
                    break;
                case 1880064: // IBM XDF, 3,5", real number of sectors
                    _imageInfo.sectors = 670;
                    _imageInfo.sectorSize = 8192; // Biggest sector size
                    differentTrackZeroSize = true;
                    break;
                default:
                    _imageInfo.sectors = _imageInfo.imageSize / _imageInfo.sectorSize;
                    break;
            }

            _imageInfo.diskType = CalculateDiskType();

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return _imageInfo.imageHasPartitions;
        }

        public override UInt64 GetImageSize()
        {
            return _imageInfo.imageSize;
        }

        public override UInt64 GetSectors()
        {
            return _imageInfo.sectors;
        }

        public override UInt32 GetSectorSize()
        {
            return _imageInfo.sectorSize;
        }

        public override byte[] ReadSector(UInt64 sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(UInt64 sectorAddress, UInt32 length)
        {
            if (differentTrackZeroSize)
            {
                throw new NotImplementedException("Not yet implemented");
            }
            else
            {
                if (sectorAddress > _imageInfo.sectors - 1)
                    throw new ArgumentOutOfRangeException("sectorAddress", "Sector address not found");

                if (sectorAddress + length > _imageInfo.sectors)
                    throw new ArgumentOutOfRangeException("length", "Requested more sectors than available");

                byte[] buffer = new byte[length * _imageInfo.sectorSize];

                FileStream stream = new FileStream(rawImagePath, FileMode.Open, FileAccess.Read);

                stream.Seek((long)(sectorAddress * _imageInfo.sectorSize), SeekOrigin.Begin);

                stream.Read(buffer, 0, (int)(length * _imageInfo.sectorSize));

                stream.Close();

                return buffer;

            }
        }

        public override string   GetImageFormat()
        { 
            return "Raw disk image (sector by sector copy)";
        }

        public override DateTime GetImageCreationTime()
        {
            return _imageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return _imageInfo.imageLastModificationTime;
        }

        public override string   GetImageName()
        {
            return _imageInfo.imageName;
        }

        public override DiskType GetDiskType()
        {
            return _imageInfo.diskType;
        }

        #region Private methods
        private DiskType CalculateDiskType()
        {
            if (_imageInfo.sectorSize == 2048)
            {
                if (_imageInfo.sectors <= 360000)
                    return DiskType.CD;
                if (_imageInfo.sectors <= 2295104)
                    return DiskType.DVDPR;
                if (_imageInfo.sectors <= 2298496)
                    return DiskType.DVDR;
                if (_imageInfo.sectors <= 4171712)
                    return DiskType.DVDRDL;
                if (_imageInfo.sectors <= 4173824)
                    return DiskType.DVDPRDL;
                if (_imageInfo.sectors <= 24438784)
                    return DiskType.BDR;
                if (_imageInfo.sectors <= 62500864)
                    return DiskType.BDRXL;
                return DiskType.Unknown;
            }
            else
            {
                switch (_imageInfo.imageSize)
                {
                    case 80384:
                        return DiskType.ECMA_66;
                    case 81664:
                        return DiskType.IBM23FD;
                    case 92160:
                        return DiskType.ATARI_525_SD;
                    case 102400:
                        return DiskType.ACORN_525_SS_SD_40;
                    case 116480:
                        return DiskType.Apple32SS;
                    case 133120:
                        return DiskType.ATARI_525_ED;
                    case 143360:
                        return DiskType.Apple33SS;
                    case 163840:
                        return DiskType.DOS_525_SS_DD_8;
                    case 184320:
                        return DiskType.DOS_525_SS_DD_9;
                    case 204800:
                        return DiskType.ACORN_525_SS_SD_80;
                    case 232960:
                        return DiskType.Apple32DS;
                    case 242944:
                        return DiskType.IBM33FD_128;
                    case 256256:
                        return DiskType.ECMA_54;
                    case 286720:
                        return DiskType.Apple33DS;
                    case 287488:
                        return DiskType.IBM33FD_256;
                    case 306432:
                        return DiskType.IBM33FD_512;
                    case 325632:
                        return DiskType.ECMA_70;
                    case 327680:
                        return DiskType.DOS_525_DS_DD_8;
                    case 368640:
                        return DiskType.DOS_525_DS_DD_9;
                    case 409600:
                        return DiskType.AppleSonySS;
                    case 495872:
                        return DiskType.IBM43FD_128;
                    case 512512:
                        return DiskType.ECMA_59;
                    case 653312:
                        return DiskType.ECMA_78;
                    case 655360:
                        return DiskType.ACORN_525_DS_DD;
                    case 737280:
                        return DiskType.DOS_35_DS_DD_9;
                    case 819200:
                        return DiskType.AppleSonyDS;
                    case 839680:
                        return DiskType.FDFORMAT_35_DD;
                    case 901120:
                        return DiskType.CBM_AMIGA_35_DD;
                    case 988416:
                        return DiskType.IBM43FD_256;
                    case 995072:
                        return DiskType.IBM53FD_256;
                    case 1021696:
                        return DiskType.ECMA_99_26;
                    case 1146624:
                        return DiskType.IBM53FD_512;
                    case 1177344:
                        return DiskType.ECMA_99_15;
                    case 1222400:
                        return DiskType.IBM53FD_1024;
                    case 1228800:
                        return DiskType.DOS_525_HD;
                    case 1255168:
                        return DiskType.ECMA_69_8;
                    case 1261568:
                        return DiskType.NEC_8_DD;
                    case 1304320:
                        return DiskType.ECMA_99_8;
                    case 1310720:
                        return DiskType.NEC_525_HD;
                    case 1427456:
                        return DiskType.FDFORMAT_525_HD;
                    case 1474560:
                        return DiskType.DOS_35_HD;
                    case 1720320:
                        return DiskType.DMF;
                    case 1763328:
                        return DiskType.FDFORMAT_35_HD;
                    case 1802240:
                        return DiskType.CBM_AMIGA_35_HD;
                    case 1880064:
                        return DiskType.XDF_35;
                    case 1884160:
                        return DiskType.XDF_35;
                    case 2949120:
                        return DiskType.DOS_35_ED;
                    case 128000000:
                        return DiskType.ECMA_154;
                    case 229632000:
                        return DiskType.ECMA_201;
                    case 481520640:
                        return DiskType.ECMA_183_512;
                    case 533403648:
                        return DiskType.ECMA_183_1024;
                    case 596787200:
                        return DiskType.ECMA_184_512;
                    case 654540800:
                        return DiskType.ECMA_184_1024;
                    default:
                        return DiskType.GENERIC_HDD;
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

        public override string   GetImageVersion()
        {
            return _imageInfo.imageVersion;
        }

        public override string   GetImageApplication()
        {
            return _imageInfo.imageApplication;
        }

        public override string   GetImageApplicationVersion()
        {
            return _imageInfo.imageApplicationVersion;
        }

        public override byte[] ReadDiskTag(DiskTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetImageCreator()
        {
            return _imageInfo.imageCreator;
        }

        public override string   GetImageComments()
        {
            return _imageInfo.imageComments;
        }

        public override string   GetDiskManufacturer()
        {
            return _imageInfo.diskManufacturer;
        }

        public override string   GetDiskModel()
        {
            return _imageInfo.diskModel;
        }

        public override string   GetDiskSerialNumber()
        {
            return _imageInfo.diskSerialNumber;
        }

        public override string   GetDiskBarcode()
        {
            return _imageInfo.diskBarcode;
        }

        public override string   GetDiskPartNumber()
        {
            return _imageInfo.diskPartNumber;
        }

        public override int      GetDiskSequence()
        {
            return _imageInfo.diskSequence;
        }

        public override int      GetLastDiskSequence()
        {
            return _imageInfo.lastDiskSequence;
        }

        public override string GetDriveManufacturer()
        {
            return _imageInfo.driveManufacturer;
        }

        public override string GetDriveModel()
        {
            return _imageInfo.driveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return _imageInfo.driveSerialNumber;
        }

        public override List<PartPlugins.Partition> GetPartitions()
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

        public override List<Track> GetSessionTracks(UInt16 session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Session> GetSessions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSector(UInt64 sectorAddress, UInt32 track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(UInt64 sectorAddress, UInt32 track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(UInt64 sectorAddress, UInt32 length, UInt32 track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, UInt32 track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(UInt64 sectorAddress, UInt32 track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length, UInt32 track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        #endregion Unsupported features
    }
}

