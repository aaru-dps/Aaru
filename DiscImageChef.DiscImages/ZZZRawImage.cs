// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ZZZRawImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class ZZZRawImage : ImagePlugin
    {
        #region Internal variables
        Filter rawImageFilter;
        bool differentTrackZeroSize;
        string extension;
        #endregion

        public ZZZRawImage()
        {
            Name = "Raw Disk Image";
            // Non-random UUID to recognize this specific plugin
            PluginUuid = new Guid("12345678-AAAA-BBBB-CCCC-123456789000");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = false;
            ImageInfo.ImageHasSessions = false;
            ImageInfo.ImageVersion = null;
            ImageInfo.ImageApplication = null;
            ImageInfo.ImageApplicationVersion = null;
            ImageInfo.ImageCreator = null;
            ImageInfo.ImageComments = null;
            ImageInfo.MediaManufacturer = null;
            ImageInfo.MediaModel = null;
            ImageInfo.MediaSerialNumber = null;
            ImageInfo.MediaBarcode = null;
            ImageInfo.MediaPartNumber = null;
            ImageInfo.MediaSequence = 0;
            ImageInfo.LastMediaSequence = 0;
            ImageInfo.DriveManufacturer = null;
            ImageInfo.DriveModel = null;
            ImageInfo.DriveSerialNumber = null;
            ImageInfo.DriveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            // Check if file is not multiple of 512
            if(imageFilter.GetDataForkLength() % 512 == 0) return true;

            extension = Path.GetExtension(imageFilter.GetFilename())?.ToLower();

            if(extension == ".hdf" && ImageInfo.ImageSize % 256 == 0) return true;

            // Check known disk sizes with sectors smaller than 512
            switch(imageFilter.GetDataForkLength())
            {
                #region Commodore
                case 174848:
                case 175531:
                case 197376:
                case 351062:
                case 822400:
                #endregion Commodore

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
                case 1255168: return true;
                default: return false;
            }
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            extension = Path.GetExtension(imageFilter.GetFilename())?.ToLower();
            switch(extension) {
                case ".iso" when imageFilter.GetDataForkLength() % 2048 == 0: ImageInfo.SectorSize = 2048;
                    break;
                case ".d81" when imageFilter.GetDataForkLength() == 819200: ImageInfo.SectorSize = 256;
                    break;
                default:
                    if((extension == ".adf" || extension == ".adl" || extension == ".ssd" || extension == ".dsd") &&
                       (imageFilter.GetDataForkLength() == 163840 || imageFilter.GetDataForkLength() == 327680 ||
                        imageFilter.GetDataForkLength() == 655360)) ImageInfo.SectorSize = 256;
                    else if((extension == ".adf" || extension == ".adl") && imageFilter.GetDataForkLength() == 819200)
                        ImageInfo.SectorSize = 1024;
                    else
                        switch(imageFilter.GetDataForkLength())
                        {
                            case 242944:
                            case 256256:
                            case 495872:
                            case 92160:
                            case 133120:
                                ImageInfo.SectorSize = 128;
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
                            case 655360:
                            case 80384: // T0S0 = 128bps
                            case 325632: // T0S0 = 128bps, T0S1 = 256bps
                            case 653312: // T0S0 = 128bps, T0S1 = 256bps

                            #region Commodore
                            case 174848:
                            case 175531:
                            case 196608:
                            case 197376:
                            case 349696:
                            case 351062:
                            case 822400:
                                #endregion Commodore

                                ImageInfo.SectorSize = 256;
                                break;
                            case 81664:
                                ImageInfo.SectorSize = 319;
                                break;
                            case 306432: // T0S0 = 128bps
                            case 1146624: // T0S0 = 128bps, T0S1 = 256bps
                            case 1177344: // T0S0 = 128bps, T0S1 = 256bps
                                ImageInfo.SectorSize = 512;
                                break;
                            case 1222400: // T0S0 = 128bps, T0S1 = 256bps
                            case 1304320: // T0S0 = 128bps, T0S1 = 256bps
                            case 1255168: // T0S0 = 128bps, T0S1 = 256bps
                            case 1261568:
                            case 1638400:
                                ImageInfo.SectorSize = 1024;
                                break;
                            default:
                                ImageInfo.SectorSize = 512;
                                break;
                        }

                    break;
            }

            ImageInfo.ImageSize = (ulong)imageFilter.GetDataForkLength();
            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            differentTrackZeroSize = false;
            rawImageFilter = imageFilter;

            switch(imageFilter.GetDataForkLength())
            {
                case 242944:
                    ImageInfo.Sectors = 1898;
                    break;
                case 256256:
                    ImageInfo.Sectors = 2002;
                    break;
                case 495872:
                    ImageInfo.Sectors = 3874;
                    break;
                case 116480:
                    ImageInfo.Sectors = 455;
                    break;
                case 287488: // T0S0 = 128bps
                    ImageInfo.Sectors = 1136;
                    differentTrackZeroSize = true;
                    break;
                case 988416: // T0S0 = 128bps
                    ImageInfo.Sectors = 3874;
                    differentTrackZeroSize = true;
                    break;
                case 995072: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.Sectors = 3900;
                    differentTrackZeroSize = true;
                    break;
                case 1021696: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.Sectors = 4004;
                    differentTrackZeroSize = true;
                    break;
                case 81664:
                    ImageInfo.Sectors = 256;
                    break;
                case 306432: // T0S0 = 128bps
                    ImageInfo.Sectors = 618;
                    differentTrackZeroSize = true;
                    break;
                case 1146624: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.Sectors = 2272;
                    differentTrackZeroSize = true;
                    break;
                case 1177344: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.Sectors = 2332;
                    differentTrackZeroSize = true;
                    break;
                case 1222400: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.Sectors = 1236;
                    differentTrackZeroSize = true;
                    break;
                case 1304320: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.Sectors = 1316;
                    differentTrackZeroSize = true;
                    break;
                case 1255168: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.Sectors = 1268;
                    differentTrackZeroSize = true;
                    break;
                case 80384: // T0S0 = 128bps
                    ImageInfo.Sectors = 322;
                    differentTrackZeroSize = true;
                    break;
                case 325632: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.Sectors = 1280;
                    differentTrackZeroSize = true;
                    break;
                case 653312: // T0S0 = 128bps, T0S1 = 256bps
                    ImageInfo.Sectors = 2560;
                    differentTrackZeroSize = true;
                    break;
                case 1880064: // IBM XDF, 3,5", real number of sectors
                    ImageInfo.Sectors = 670;
                    ImageInfo.SectorSize = 8192; // Biggest sector size
                    differentTrackZeroSize = true;
                    break;
                case 175531:
                    ImageInfo.Sectors = 683;
                    break;
                case 197375:
                    ImageInfo.Sectors = 768;
                    break;
                case 351062:
                    ImageInfo.Sectors = 1366;
                    break;
                case 822400:
                    ImageInfo.Sectors = 3200;
                    break;
                default:
                    ImageInfo.Sectors = ImageInfo.ImageSize / ImageInfo.SectorSize;
                    break;
            }

            ImageInfo.MediaType = CalculateDiskType();

            switch(ImageInfo.MediaType)
            {
                case MediaType.CD:
                case MediaType.DVDPR:
                case MediaType.DVDR:
                case MediaType.DVDRDL:
                case MediaType.DVDPRDL:
                case MediaType.BDR:
                case MediaType.BDRXL:
                    ImageInfo.XmlMediaType = XmlMediaType.OpticalDisc;
                    break;
                default:
                    ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                    break;
            }

            // Sharp X68000 SASI hard disks
            if(extension == ".hdf")
                if(ImageInfo.ImageSize % 256 == 0)
                {
                    ImageInfo.SectorSize = 256;
                    ImageInfo.Sectors = ImageInfo.ImageSize / ImageInfo.SectorSize;
                    ImageInfo.MediaType = MediaType.GENERIC_HDD;
                }

            if(ImageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                ImageInfo.ImageHasSessions = true;
                ImageInfo.ImageHasPartitions = true;
            }

            DicConsole.VerboseWriteLine("Raw disk image contains a disk of type {0}", ImageInfo.MediaType);

            switch(ImageInfo.MediaType)
            {
                case MediaType.ACORN_35_DS_DD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 5;
                    break;
                case MediaType.ACORN_35_DS_HD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.ACORN_525_DS_DD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.ACORN_525_SS_DD_40:
                    ImageInfo.Cylinders = 40;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.ACORN_525_SS_DD_80:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.ACORN_525_SS_SD_40:
                    ImageInfo.Cylinders = 40;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.ACORN_525_SS_SD_80:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.Apple32DS:
                    ImageInfo.Cylinders = 35;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 13;
                    break;
                case MediaType.Apple32SS:
                    ImageInfo.Cylinders = 36;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 13;
                    break;
                case MediaType.Apple33DS:
                    ImageInfo.Cylinders = 35;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.Apple33SS:
                    ImageInfo.Cylinders = 35;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.AppleSonyDS:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.AppleSonySS:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.ATARI_35_DS_DD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.ATARI_35_DS_DD_11:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 11;
                    break;
                case MediaType.ATARI_35_SS_DD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.ATARI_35_SS_DD_11:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 11;
                    break;
                case MediaType.ATARI_525_ED:
                    ImageInfo.Cylinders = 40;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.ATARI_525_SD:
                    ImageInfo.Cylinders = 40;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 18;
                    break;
                case MediaType.CBM_35_DD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.CBM_AMIGA_35_DD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 11;
                    break;
                case MediaType.CBM_AMIGA_35_HD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 22;
                    break;
                case MediaType.DMF:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 21;
                    break;
                case MediaType.DOS_35_DS_DD_9:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.Apricot_35:
                    ImageInfo.Cylinders = 70;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_35_ED:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 36;
                    break;
                case MediaType.DOS_35_HD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 18;
                    break;
                case MediaType.DOS_35_SS_DD_9:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_525_DS_DD_8:
                    ImageInfo.Cylinders = 40;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.DOS_525_DS_DD_9:
                    ImageInfo.Cylinders = 40;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_525_HD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.DOS_525_SS_DD_8:
                    ImageInfo.Cylinders = 40;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.DOS_525_SS_DD_9:
                    ImageInfo.Cylinders = 40;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.ECMA_54:
                    ImageInfo.Cylinders = 77;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.ECMA_59:
                    ImageInfo.Cylinders = 77;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.ECMA_66:
                    ImageInfo.Cylinders = 35;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.ECMA_69_8:
                    ImageInfo.Cylinders = 77;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.ECMA_70:
                    ImageInfo.Cylinders = 40;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.ECMA_78:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.ECMA_99_15:
                    ImageInfo.Cylinders = 77;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.ECMA_99_26:
                    ImageInfo.Cylinders = 77;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.ECMA_99_8:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.FDFORMAT_35_DD:
                    ImageInfo.Cylinders = 82;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.FDFORMAT_35_HD:
                    ImageInfo.Cylinders = 82;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 21;
                    break;
                case MediaType.FDFORMAT_525_HD:
                    ImageInfo.Cylinders = 82;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 17;
                    break;
                case MediaType.IBM23FD:
                    ImageInfo.Cylinders = 32;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.IBM33FD_128:
                    ImageInfo.Cylinders = 73;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.IBM33FD_256:
                    ImageInfo.Cylinders = 74;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.IBM33FD_512:
                    ImageInfo.Cylinders = 74;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.IBM43FD_128:
                    ImageInfo.Cylinders = 74;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.IBM43FD_256:
                    ImageInfo.Cylinders = 74;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.IBM53FD_1024:
                    ImageInfo.Cylinders = 74;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.IBM53FD_256:
                    ImageInfo.Cylinders = 74;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.IBM53FD_512:
                    ImageInfo.Cylinders = 74;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.NEC_35_TD:
                    ImageInfo.Cylinders = 240;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 38;
                    break;
                case MediaType.NEC_525_HD:
                    ImageInfo.Cylinders = 77;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.XDF_35:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 23;
                    break;
                // Following ones are what the device itself report, not the physical geometry
                case MediaType.Jaz:
                    ImageInfo.Cylinders = 1021;
                    ImageInfo.Heads = 64;
                    ImageInfo.SectorsPerTrack = 32;
                    break;
                case MediaType.PocketZip:
                    ImageInfo.Cylinders = 154;
                    ImageInfo.Heads = 16;
                    ImageInfo.SectorsPerTrack = 32;
                    break;
                case MediaType.LS120:
                    ImageInfo.Cylinders = 963;
                    ImageInfo.Heads = 8;
                    ImageInfo.SectorsPerTrack = 32;
                    break;
                case MediaType.LS240:
                    ImageInfo.Cylinders = 262;
                    ImageInfo.Heads = 32;
                    ImageInfo.SectorsPerTrack = 56;
                    break;
                case MediaType.FD32MB:
                    ImageInfo.Cylinders = 1024;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 32;
                    break;
                default:
                    ImageInfo.Cylinders = (uint)(ImageInfo.Sectors / 16 / 63);
                    ImageInfo.Heads = 16;
                    ImageInfo.SectorsPerTrack = 63;
                    break;
            }

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.ImageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.ImageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.Sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.SectorSize;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(differentTrackZeroSize) throw new NotImplementedException("Not yet implemented");

            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.SectorSize];

            Stream stream = rawImageFilter.GetDataForkStream();

            stream.Seek((long)(sectorAddress * ImageInfo.SectorSize), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * ImageInfo.SectorSize));

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "Raw disk image (sector by sector copy)";
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.ImageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.ImageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }

        public override List<Track> GetTracks()
        {
            if(ImageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            Track trk = new Track();
            trk.TrackBytesPerSector = (int)ImageInfo.SectorSize;
            trk.TrackEndSector = ImageInfo.Sectors - 1;
            trk.TrackFile = rawImageFilter.GetFilename();
            trk.TrackFileOffset = 0;
            trk.TrackFileType = "BINARY";
            trk.TrackRawBytesPerSector = (int)ImageInfo.SectorSize;
            trk.TrackSequence = 1;
            trk.TrackStartSector = 0;
            trk.TrackSubchannelType = TrackSubchannelType.None;
            trk.TrackType = TrackType.Data;
            trk.TrackSession = 1;
            List<Track> lst = new List<Track>();
            lst.Add(trk);
            return lst;
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            if(ImageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(session.SessionSequence != 1)
                throw new ArgumentOutOfRangeException(nameof(session), "Only a single session is supported");

            Track trk = new Track();
            trk.TrackBytesPerSector = (int)ImageInfo.SectorSize;
            trk.TrackEndSector = ImageInfo.Sectors - 1;
            trk.TrackFilter = rawImageFilter;
            trk.TrackFile = rawImageFilter.GetFilename();
            trk.TrackFileOffset = 0;
            trk.TrackFileType = "BINARY";
            trk.TrackRawBytesPerSector = (int)ImageInfo.SectorSize;
            trk.TrackSequence = 1;
            trk.TrackStartSector = 0;
            trk.TrackSubchannelType = TrackSubchannelType.None;
            trk.TrackType = TrackType.Data;
            trk.TrackSession = 1;
            List<Track> lst = new List<Track>();
            lst.Add(trk);
            return lst;
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            if(ImageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(session != 1)
                throw new ArgumentOutOfRangeException(nameof(session), "Only a single session is supported");

            Track trk = new Track();
            trk.TrackBytesPerSector = (int)ImageInfo.SectorSize;
            trk.TrackEndSector = ImageInfo.Sectors - 1;
            trk.TrackFilter = rawImageFilter;
            trk.TrackFile = rawImageFilter.GetFilename();
            trk.TrackFileOffset = 0;
            trk.TrackFileType = "BINARY";
            trk.TrackRawBytesPerSector = (int)ImageInfo.SectorSize;
            trk.TrackSequence = 1;
            trk.TrackStartSector = 0;
            trk.TrackSubchannelType = TrackSubchannelType.None;
            trk.TrackType = TrackType.Data;
            trk.TrackSession = 1;
            List<Track> lst = new List<Track>();
            lst.Add(trk);
            return lst;
        }

        public override List<Session> GetSessions()
        {
            if(ImageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            Session sess = new Session();
            sess.EndSector = ImageInfo.Sectors - 1;
            sess.EndTrack = 1;
            sess.SessionSequence = 1;
            sess.StartSector = 0;
            sess.StartTrack = 1;
            List<Session> lst = new List<Session>();
            lst.Add(sess);
            return lst;
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            if(ImageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(track != 1) throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSector(sectorAddress);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(ImageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(track != 1) throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectors(sectorAddress, length);
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            if(ImageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(track != 1) throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSector(sectorAddress);
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(ImageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(track != 1) throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectors(sectorAddress, length);
        }

        #region Private methods
        MediaType CalculateDiskType()
        {
            if(ImageInfo.SectorSize == 2048)
            {
                if(ImageInfo.Sectors <= 360000) return MediaType.CD;
                if(ImageInfo.Sectors <= 2295104) return MediaType.DVDPR;
                if(ImageInfo.Sectors <= 2298496) return MediaType.DVDR;
                if(ImageInfo.Sectors <= 4171712) return MediaType.DVDRDL;
                if(ImageInfo.Sectors <= 4173824) return MediaType.DVDPRDL;
                if(ImageInfo.Sectors <= 24438784) return MediaType.BDR;
                if(ImageInfo.Sectors <= 62500864) return MediaType.BDRXL;

                return MediaType.Unknown;
            }

            switch(ImageInfo.ImageSize)
            {
                case 80384: return MediaType.ECMA_66;
                case 81664: return MediaType.IBM23FD;
                case 92160: return MediaType.ATARI_525_SD;
                case 102400: return MediaType.ACORN_525_SS_SD_40;
                case 116480: return MediaType.Apple32SS;
                case 133120: return MediaType.ATARI_525_ED;
                case 143360: return MediaType.Apple33SS;
                case 163840:
                    if(ImageInfo.SectorSize == 256) return MediaType.ACORN_525_SS_DD_40;

                    return MediaType.DOS_525_SS_DD_8;
                case 184320: return MediaType.DOS_525_SS_DD_9;
                case 204800: return MediaType.ACORN_525_SS_SD_80;
                case 232960: return MediaType.Apple32DS;
                case 242944: return MediaType.IBM33FD_128;
                case 256256: return MediaType.ECMA_54;
                case 286720: return MediaType.Apple33DS;
                case 287488: return MediaType.IBM33FD_256;
                case 306432: return MediaType.IBM33FD_512;
                case 322560: return MediaType.Apricot_35;
                case 325632: return MediaType.ECMA_70;
                case 327680:
                    if(ImageInfo.SectorSize == 256) return MediaType.ACORN_525_SS_DD_80;

                    return MediaType.DOS_525_DS_DD_8;
                case 368640:
                    if(extension == ".st") return MediaType.DOS_35_SS_DD_9;

                    return MediaType.DOS_525_DS_DD_9;
                case 409600:
                    if(extension == ".st") return MediaType.ATARI_35_SS_DD;

                    return MediaType.AppleSonySS;
                case 450560: return MediaType.ATARI_35_SS_DD_11;
                case 495872: return MediaType.IBM43FD_128;
                case 512512: return MediaType.ECMA_59;
                case 653312: return MediaType.ECMA_78;
                case 655360: return MediaType.ACORN_525_DS_DD;
                case 737280: return MediaType.DOS_35_DS_DD_9;
                case 819200:
                    if(ImageInfo.SectorSize == 256) return MediaType.CBM_35_DD;
                    if((extension == ".adf" || extension == ".adl") && ImageInfo.SectorSize == 1024)
                        return MediaType.ACORN_35_DS_DD;
                    if(extension == ".st") return MediaType.ATARI_35_DS_DD;

                    return MediaType.AppleSonyDS;
                case 839680: return MediaType.FDFORMAT_35_DD;
                case 901120:
                    if(extension == ".st") return MediaType.ATARI_35_DS_DD_11;

                    return MediaType.CBM_AMIGA_35_DD;
                case 988416: return MediaType.IBM43FD_256;
                case 995072: return MediaType.IBM53FD_256;
                case 1021696: return MediaType.ECMA_99_26;
                case 1146624: return MediaType.IBM53FD_512;
                case 1177344: return MediaType.ECMA_99_15;
                case 1222400: return MediaType.IBM53FD_1024;
                case 1228800: return MediaType.DOS_525_HD;
                case 1255168: return MediaType.ECMA_69_8;
                case 1261568: return MediaType.NEC_525_HD;
                case 1304320: return MediaType.ECMA_99_8;
                case 1427456: return MediaType.FDFORMAT_525_HD;
                case 1474560: return MediaType.DOS_35_HD;
                case 1638400: return MediaType.ACORN_35_DS_HD;
                case 1720320: return MediaType.DMF;
                case 1763328: return MediaType.FDFORMAT_35_HD;
                case 1802240: return MediaType.CBM_AMIGA_35_HD;
                case 1880064: return MediaType.XDF_35;
                case 1884160: return MediaType.XDF_35;
                case 2949120: return MediaType.DOS_35_ED;
                case 9338880: return MediaType.NEC_35_TD;
                case 33554432: return MediaType.FD32MB;
                case 40387584: return MediaType.PocketZip;
                case 126222336: return MediaType.LS120;
                case 127923200: return MediaType.ECMA_154;
                case 201410560: return MediaType.HiFD;
                case 229632000: return MediaType.ECMA_201;
                case 240386048: return MediaType.LS240;
                case 481520640: return MediaType.ECMA_183_512;
                case 533403648: return MediaType.ECMA_183;
                case 596787200: return MediaType.ECMA_184_512;
                case 654540800: return MediaType.ECMA_184;
                case 1070617600: return MediaType.Jaz;

                #region Commodore
                case 174848:
                case 175531: return MediaType.CBM_1540;
                case 196608:
                case 197376: return MediaType.CBM_1540_Ext;
                case 349696:
                case 351062: return MediaType.CBM_1571;
                #endregion Commodore

                default: return MediaType.GENERIC_HDD;
            }
        }
        #endregion

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

        public override string GetImageVersion()
        {
            return ImageInfo.ImageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.ImageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.ImageApplicationVersion;
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.MediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.MediaModel;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.MediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.MediaBarcode;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.MediaPartNumber;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.MediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.LastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.DriveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.DriveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.DriveSerialNumber;
        }

        public override List<Partition> GetPartitions()
        {
            if(ImageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            List<Partition> parts = new List<Partition>();
            Partition part = new Partition
            {
                Start = 0,
                Length = ImageInfo.Sectors,
                Offset = 0,
                Sequence = 0,
                Type = "MODE1/2048",
                Size = ImageInfo.Sectors * ImageInfo.SectorSize
            };
            parts.Add(part);
            return parts;
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        #endregion Unsupported features
    }
}