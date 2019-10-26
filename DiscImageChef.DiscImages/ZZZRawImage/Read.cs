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
//     Reads raw image, that is, user data sector by sector copy.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.SCSI;
using Schemas;
using DMI = DiscImageChef.Decoders.Xbox.DMI;
using Session = DiscImageChef.CommonTypes.Structs.Session;
using TrackType = DiscImageChef.CommonTypes.Enums.TrackType;

namespace DiscImageChef.DiscImages
{
    public partial class ZZZRawImage
    {
        public bool Open(IFilter imageFilter)
        {
            var stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            extension = Path.GetExtension(imageFilter.GetFilename())?.ToLower();
            switch (extension)
            {
                case ".iso" when imageFilter.GetDataForkLength() % 2048 == 0:
                    imageInfo.SectorSize = 2048;
                    break;
                case ".d81" when imageFilter.GetDataForkLength() == 819200:
                    imageInfo.SectorSize = 256;
                    break;
                default:
                    if ((extension == ".adf" || extension == ".adl" || extension == ".ssd" || extension == ".dsd") &&
                        (imageFilter.GetDataForkLength() == 163840 || imageFilter.GetDataForkLength() == 327680 ||
                         imageFilter.GetDataForkLength() == 655360)) imageInfo.SectorSize = 256;
                    else if ((extension == ".adf" || extension == ".adl") && imageFilter.GetDataForkLength() == 819200)
                        imageInfo.SectorSize = 1024;
                    else
                        switch (imageFilter.GetDataForkLength())
                        {
                            case 242944:
                            case 256256:
                            case 495872:
                            case 92160:
                            case 133120:
                                imageInfo.SectorSize = 128;
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

                                imageInfo.SectorSize = 256;
                                break;
                            case 81664:
                                imageInfo.SectorSize = 319;
                                break;
                            case 306432: // T0S0 = 128bps
                            case 1146624: // T0S0 = 128bps, T0S1 = 256bps
                            case 1177344: // T0S0 = 128bps, T0S1 = 256bps
                                imageInfo.SectorSize = 512;
                                break;
                            case 1222400: // T0S0 = 128bps, T0S1 = 256bps
                            case 1304320: // T0S0 = 128bps, T0S1 = 256bps
                            case 1255168: // T0S0 = 128bps, T0S1 = 256bps
                            case 1261568:
                            case 1638400:
                                imageInfo.SectorSize = 1024;
                                break;
                            case 35002122240:
                                imageInfo.SectorSize = 2048;
                                break;
                            default:
                                imageInfo.SectorSize = 512;
                                break;
                        }

                    break;
            }

            imageInfo.ImageSize = (ulong) imageFilter.GetDataForkLength();
            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            differentTrackZeroSize = false;
            rawImageFilter = imageFilter;

            switch (imageFilter.GetDataForkLength())
            {
                case 242944:
                    imageInfo.Sectors = 1898;
                    break;
                case 256256:
                    imageInfo.Sectors = 2002;
                    break;
                case 495872:
                    imageInfo.Sectors = 3874;
                    break;
                case 116480:
                    imageInfo.Sectors = 455;
                    break;
                case 287488: // T0S0 = 128bps
                    imageInfo.Sectors = 1136;
                    differentTrackZeroSize = true;
                    break;
                case 988416: // T0S0 = 128bps
                    imageInfo.Sectors = 3874;
                    differentTrackZeroSize = true;
                    break;
                case 995072: // T0S0 = 128bps, T0S1 = 256bps
                    imageInfo.Sectors = 3900;
                    differentTrackZeroSize = true;
                    break;
                case 1021696: // T0S0 = 128bps, T0S1 = 256bps
                    imageInfo.Sectors = 4004;
                    differentTrackZeroSize = true;
                    break;
                case 81664:
                    imageInfo.Sectors = 256;
                    break;
                case 306432: // T0S0 = 128bps
                    imageInfo.Sectors = 618;
                    differentTrackZeroSize = true;
                    break;
                case 1146624: // T0S0 = 128bps, T0S1 = 256bps
                    imageInfo.Sectors = 2272;
                    differentTrackZeroSize = true;
                    break;
                case 1177344: // T0S0 = 128bps, T0S1 = 256bps
                    imageInfo.Sectors = 2332;
                    differentTrackZeroSize = true;
                    break;
                case 1222400: // T0S0 = 128bps, T0S1 = 256bps
                    imageInfo.Sectors = 1236;
                    differentTrackZeroSize = true;
                    break;
                case 1304320: // T0S0 = 128bps, T0S1 = 256bps
                    imageInfo.Sectors = 1316;
                    differentTrackZeroSize = true;
                    break;
                case 1255168: // T0S0 = 128bps, T0S1 = 256bps
                    imageInfo.Sectors = 1268;
                    differentTrackZeroSize = true;
                    break;
                case 80384: // T0S0 = 128bps
                    imageInfo.Sectors = 322;
                    differentTrackZeroSize = true;
                    break;
                case 325632: // T0S0 = 128bps, T0S1 = 256bps
                    imageInfo.Sectors = 1280;
                    differentTrackZeroSize = true;
                    break;
                case 653312: // T0S0 = 128bps, T0S1 = 256bps
                    imageInfo.Sectors = 2560;
                    differentTrackZeroSize = true;
                    break;
                case 1880064: // IBM XDF, 3,5", real number of sectors
                    imageInfo.Sectors = 670;
                    imageInfo.SectorSize = 8192; // Biggest sector size
                    differentTrackZeroSize = true;
                    break;
                case 175531:
                    imageInfo.Sectors = 683;
                    break;
                case 197375:
                    imageInfo.Sectors = 768;
                    break;
                case 351062:
                    imageInfo.Sectors = 1366;
                    break;
                case 822400:
                    imageInfo.Sectors = 3200;
                    break;
                default:
                    imageInfo.Sectors = imageInfo.ImageSize / imageInfo.SectorSize;
                    break;
            }

            imageInfo.MediaType = CalculateDiskType();

            if (imageInfo.ImageSize % 2352 == 0 || imageInfo.ImageSize % 2448 == 0)
            {
                var sync = new byte[12];
                var header = new byte[4];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(sync, 0, 12);
                stream.Read(header, 0, 4);
                if (cdSync.SequenceEqual(sync))
                {
                    rawCompactDisc = true;
                    hasSubchannel = imageInfo.ImageSize % 2448 == 0;
                    imageInfo.Sectors = imageInfo.ImageSize / (ulong) (hasSubchannel ? 2448 : 2352);
                    imageInfo.MediaType = MediaType.CD;
                    mode2 = header[3] == 0x02;
                    imageInfo.SectorSize = (uint) (mode2 ? 2336 : 2048);
                }
            }

            // Sharp X68000 SASI hard disks
            if (extension == ".hdf")
                if (imageInfo.ImageSize % 256 == 0)
                {
                    imageInfo.SectorSize = 256;
                    imageInfo.Sectors = imageInfo.ImageSize / imageInfo.SectorSize;
                    imageInfo.MediaType = MediaType.GENERIC_HDD;
                }

            // Search for known tags
            var basename = imageFilter.GetBasePath();
            basename = basename.Substring(0, basename.Length - extension.Length);

            mediaTags = new Dictionary<MediaTagType, byte[]>();
            foreach (var sidecar in readWriteSidecars)
                try
                {
                    var filters = new FiltersList();
                    var filter = filters.GetFilter(basename + sidecar.name);
                    if (filter == null || !filter.IsOpened()) continue;

                    DicConsole.DebugWriteLine("ZZZRawImage Plugin", "Found media tag {0}", sidecar.tag);
                    var data = new byte[filter.GetDataForkLength()];
                    filter.GetDataForkStream().Read(data, 0, data.Length);
                    mediaTags.Add(sidecar.tag, data);
                }
                catch (IOException)
                {
                }

            // If there are INQUIRY and IDENTIFY tags, it's ATAPI
            if (mediaTags.ContainsKey(MediaTagType.SCSI_INQUIRY))
                if (mediaTags.TryGetValue(MediaTagType.ATA_IDENTIFY, out var tag))
                {
                    mediaTags.Remove(MediaTagType.ATA_IDENTIFY);
                    mediaTags.Add(MediaTagType.ATAPI_IDENTIFY, tag);
                }

            // It is a blu-ray
            if (mediaTags.ContainsKey(MediaTagType.BD_DI))
            {
                imageInfo.MediaType = MediaType.BDROM;

                if (mediaTags.TryGetValue(MediaTagType.DVD_BCA, out var bca))
                {
                    mediaTags.Remove(MediaTagType.DVD_BCA);
                    mediaTags.Add(MediaTagType.BD_BCA, bca);
                }

                if (mediaTags.TryGetValue(MediaTagType.DVDRAM_DDS, out var dds))
                {
                    imageInfo.MediaType = MediaType.BDRE;
                    mediaTags.Remove(MediaTagType.DVDRAM_DDS);
                    mediaTags.Add(MediaTagType.BD_DDS, dds);
                }

                if (mediaTags.TryGetValue(MediaTagType.DVDRAM_SpareArea, out var sai))
                {
                    imageInfo.MediaType = MediaType.BDRE;
                    mediaTags.Remove(MediaTagType.DVDRAM_SpareArea);
                    mediaTags.Add(MediaTagType.BD_SpareArea, sai);
                }
            }

            // It is a DVD
            if (mediaTags.TryGetValue(MediaTagType.DVD_PFI, out var pfi))
            {
                var decPfi = PFI.Decode(pfi).Value;
                switch (decPfi.DiskCategory)
                {
                    case DiskCategory.DVDPR:
                        imageInfo.MediaType = MediaType.DVDPR;
                        break;
                    case DiskCategory.DVDPRDL:
                        imageInfo.MediaType = MediaType.DVDPRDL;
                        break;
                    case DiskCategory.DVDPRW:
                        imageInfo.MediaType = MediaType.DVDPRW;
                        break;
                    case DiskCategory.DVDPRWDL:
                        imageInfo.MediaType = MediaType.DVDPRWDL;
                        break;
                    case DiskCategory.DVDR:
                        imageInfo.MediaType = decPfi.PartVersion == 6 ? MediaType.DVDRDL : MediaType.DVDR;
                        break;
                    case DiskCategory.DVDRAM:
                        imageInfo.MediaType = MediaType.DVDRAM;
                        break;
                    default:
                        imageInfo.MediaType = MediaType.DVDROM;
                        break;
                    case DiskCategory.DVDRW:
                        imageInfo.MediaType = decPfi.PartVersion == 3 ? MediaType.DVDRWDL : MediaType.DVDRW;
                        break;
                    case DiskCategory.HDDVDR:
                        imageInfo.MediaType = MediaType.HDDVDR;
                        break;
                    case DiskCategory.HDDVDRAM:
                        imageInfo.MediaType = MediaType.HDDVDRAM;
                        break;
                    case DiskCategory.HDDVDROM:
                        imageInfo.MediaType = MediaType.HDDVDROM;
                        break;
                    case DiskCategory.HDDVDRW:
                        imageInfo.MediaType = MediaType.HDDVDRW;
                        break;
                    case DiskCategory.Nintendo:
                        imageInfo.MediaType = decPfi.DiscSize == DVDSize.Eighty ? MediaType.GOD : MediaType.WOD;
                        break;
                    case DiskCategory.UMD:
                        imageInfo.MediaType = MediaType.UMD;
                        break;
                }

                if ((imageInfo.MediaType == MediaType.DVDR || imageInfo.MediaType == MediaType.DVDRW ||
                     imageInfo.MediaType == MediaType.HDDVDR) &&
                    mediaTags.TryGetValue(MediaTagType.DVD_MediaIdentifier, out var mid))
                {
                    mediaTags.Remove(MediaTagType.DVD_MediaIdentifier);
                    mediaTags.Add(MediaTagType.DVDR_MediaIdentifier, mid);
                }

                // Check for Xbox
                if (mediaTags.TryGetValue(MediaTagType.DVD_DMI, out var dmi))
                    if (DMI.IsXbox(dmi) || DMI.IsXbox360(dmi))
                        if (DMI.IsXbox(dmi))
                        {
                            imageInfo.MediaType = MediaType.XGD;
                        }
                        else if (DMI.IsXbox360(dmi))
                        {
                            imageInfo.MediaType = MediaType.XGD2;

                            // All XGD3 all have the same number of blocks
                            if (imageInfo.Sectors == 25063 || // Locked (or non compatible drive)
                                imageInfo.Sectors == 4229664 || // Xtreme unlock
                                imageInfo.Sectors == 4246304) // Wxripper unlock
                                imageInfo.MediaType = MediaType.XGD3;
                        }
            }

            // It's MultiMediaCard or SecureDigital
            if (mediaTags.ContainsKey(MediaTagType.SD_CID) || mediaTags.ContainsKey(MediaTagType.SD_CSD) ||
                mediaTags.ContainsKey(MediaTagType.SD_OCR))
            {
                imageInfo.MediaType = MediaType.SecureDigital;

                if (mediaTags.ContainsKey(MediaTagType.MMC_ExtendedCSD) || !mediaTags.ContainsKey(MediaTagType.SD_SCR))
                {
                    imageInfo.MediaType = MediaType.MMC;

                    if (mediaTags.TryGetValue(MediaTagType.SD_CID, out var cid))
                    {
                        mediaTags.Remove(MediaTagType.SD_CID);
                        mediaTags.Add(MediaTagType.MMC_CID, cid);
                    }

                    if (mediaTags.TryGetValue(MediaTagType.SD_CSD, out var csd))
                    {
                        mediaTags.Remove(MediaTagType.SD_CSD);
                        mediaTags.Add(MediaTagType.MMC_CSD, csd);
                    }

                    if (mediaTags.TryGetValue(MediaTagType.SD_OCR, out var ocr))
                    {
                        mediaTags.Remove(MediaTagType.SD_OCR);
                        mediaTags.Add(MediaTagType.MMC_OCR, ocr);
                    }
                }
            }

            // It's a compact disc
            if (mediaTags.ContainsKey(MediaTagType.CD_FullTOC))
            {
                imageInfo.MediaType = imageInfo.Sectors > 360000 ? MediaType.DDCD : MediaType.CD;

                // Only CD-R and CD-RW have ATIP
                if (mediaTags.TryGetValue(MediaTagType.CD_ATIP, out var atipBuf))
                {
                    var atip = ATIP.Decode(atipBuf);
                    if (atip.HasValue) imageInfo.MediaType = atip.Value.DiscType ? MediaType.CDRW : MediaType.CDR;
                }

                if (mediaTags.TryGetValue(MediaTagType.Floppy_LeadOut, out var leadout))
                {
                    mediaTags.Remove(MediaTagType.Floppy_LeadOut);
                    mediaTags.Add(MediaTagType.CD_LeadOut, leadout);
                }
            }

            switch (imageInfo.MediaType)
            {
                case MediaType.ACORN_35_DS_DD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 5;
                    break;
                case MediaType.ACORN_35_DS_HD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.ACORN_525_DS_DD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.ACORN_525_SS_DD_40:
                    imageInfo.Cylinders = 40;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.ACORN_525_SS_DD_80:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.ACORN_525_SS_SD_40:
                    imageInfo.Cylinders = 40;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.ACORN_525_SS_SD_80:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.Apple32DS:
                    imageInfo.Cylinders = 35;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 13;
                    break;
                case MediaType.Apple32SS:
                    imageInfo.Cylinders = 36;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 13;
                    break;
                case MediaType.Apple33DS:
                    imageInfo.Cylinders = 35;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.Apple33SS:
                    imageInfo.Cylinders = 35;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.AppleSonyDS:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.AppleSonySS:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.ATARI_35_DS_DD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.ATARI_35_DS_DD_11:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 11;
                    break;
                case MediaType.ATARI_35_SS_DD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.ATARI_35_SS_DD_11:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 11;
                    break;
                case MediaType.ATARI_525_ED:
                    imageInfo.Cylinders = 40;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.ATARI_525_SD:
                    imageInfo.Cylinders = 40;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 18;
                    break;
                case MediaType.CBM_35_DD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.CBM_AMIGA_35_DD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 11;
                    break;
                case MediaType.CBM_AMIGA_35_HD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 22;
                    break;
                case MediaType.DMF:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 21;
                    break;
                case MediaType.DOS_35_DS_DD_9:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.Apricot_35:
                    imageInfo.Cylinders = 70;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_35_ED:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 36;
                    break;
                case MediaType.DOS_35_HD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 18;
                    break;
                case MediaType.DOS_35_SS_DD_9:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_525_DS_DD_8:
                    imageInfo.Cylinders = 40;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.DOS_525_DS_DD_9:
                    imageInfo.Cylinders = 40;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_525_HD:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.DOS_525_SS_DD_8:
                    imageInfo.Cylinders = 40;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.DOS_525_SS_DD_9:
                    imageInfo.Cylinders = 40;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.ECMA_54:
                    imageInfo.Cylinders = 77;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.ECMA_59:
                    imageInfo.Cylinders = 77;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.ECMA_66:
                    imageInfo.Cylinders = 35;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.ECMA_69_8:
                    imageInfo.Cylinders = 77;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.ECMA_70:
                    imageInfo.Cylinders = 40;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.ECMA_78:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.ECMA_99_15:
                    imageInfo.Cylinders = 77;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.ECMA_99_26:
                    imageInfo.Cylinders = 77;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.ECMA_99_8:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.FDFORMAT_35_DD:
                    imageInfo.Cylinders = 82;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.FDFORMAT_35_HD:
                    imageInfo.Cylinders = 82;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 21;
                    break;
                case MediaType.FDFORMAT_525_HD:
                    imageInfo.Cylinders = 82;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 17;
                    break;
                case MediaType.IBM23FD:
                    imageInfo.Cylinders = 32;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.IBM33FD_128:
                    imageInfo.Cylinders = 73;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.IBM33FD_256:
                    imageInfo.Cylinders = 74;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.IBM33FD_512:
                    imageInfo.Cylinders = 74;
                    imageInfo.Heads = 1;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.IBM43FD_128:
                    imageInfo.Cylinders = 74;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.IBM43FD_256:
                    imageInfo.Cylinders = 74;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.IBM53FD_1024:
                    imageInfo.Cylinders = 74;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.IBM53FD_256:
                    imageInfo.Cylinders = 74;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.IBM53FD_512:
                    imageInfo.Cylinders = 74;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.NEC_35_TD:
                    imageInfo.Cylinders = 240;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 38;
                    break;
                case MediaType.NEC_525_HD:
                    imageInfo.Cylinders = 77;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.XDF_35:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 23;
                    break;
                // Following ones are what the device itself report, not the physical geometry
                case MediaType.Jaz:
                    imageInfo.Cylinders = 1021;
                    imageInfo.Heads = 64;
                    imageInfo.SectorsPerTrack = 32;
                    break;
                case MediaType.PocketZip:
                    imageInfo.Cylinders = 154;
                    imageInfo.Heads = 16;
                    imageInfo.SectorsPerTrack = 32;
                    break;
                case MediaType.LS120:
                    imageInfo.Cylinders = 963;
                    imageInfo.Heads = 8;
                    imageInfo.SectorsPerTrack = 32;
                    break;
                case MediaType.LS240:
                    imageInfo.Cylinders = 262;
                    imageInfo.Heads = 32;
                    imageInfo.SectorsPerTrack = 56;
                    break;
                case MediaType.FD32MB:
                    imageInfo.Cylinders = 1024;
                    imageInfo.Heads = 2;
                    imageInfo.SectorsPerTrack = 32;
                    break;
                case MediaType.ZIP100:
                    imageInfo.Cylinders = 96;
                    imageInfo.Heads = 64;
                    imageInfo.SectorsPerTrack = 32;
                    break;
                case MediaType.ZIP250:
                    imageInfo.Cylinders = 239;
                    imageInfo.Heads = 64;
                    imageInfo.SectorsPerTrack = 32;
                    break;
                default:
                    imageInfo.Cylinders = (uint) (imageInfo.Sectors / 16 / 63);
                    imageInfo.Heads = 16;
                    imageInfo.SectorsPerTrack = 63;
                    break;
            }

            // It's SCSI, check tags
            if (mediaTags.ContainsKey(MediaTagType.SCSI_INQUIRY))
            {
                var devType = PeripheralDeviceTypes.DirectAccess;
                Inquiry.SCSIInquiry? scsiInq = null;
                if (mediaTags.TryGetValue(MediaTagType.SCSI_INQUIRY, out var inq))
                {
                    scsiInq = Inquiry.Decode(inq);
                    devType = (PeripheralDeviceTypes) (inq[0] & 0x1F);
                }

                Modes.DecodedMode? decMode = null;

                if (mediaTags.TryGetValue(MediaTagType.SCSI_MODESENSE_6, out var mode6))
                    decMode = Modes.DecodeMode6(mode6, devType);
                else if (mediaTags.TryGetValue(MediaTagType.SCSI_MODESENSE_10, out var mode10))
                    decMode = Modes.DecodeMode10(mode10, devType);

                byte mediumType = 0;
                byte densityCode = 0;

                if (decMode.HasValue)
                {
                    mediumType = (byte) decMode.Value.Header.MediumType;
                    if (decMode.Value.Header.BlockDescriptors != null &&
                        decMode.Value.Header.BlockDescriptors.Length >= 1)
                        densityCode = (byte) decMode.Value.Header.BlockDescriptors[0].Density;

                    if (decMode.Value.Pages != null)
                        foreach (var page in decMode.Value.Pages)
                            // CD-ROM page
                            if (page.Page == 0x2A && page.Subpage == 0)
                            {
                                if (mediaTags.ContainsKey(MediaTagType.SCSI_MODEPAGE_2A))
                                    mediaTags.Remove(MediaTagType.SCSI_MODEPAGE_2A);
                                mediaTags.Add(MediaTagType.SCSI_MODEPAGE_2A, page.PageResponse);
                            }
                            // Rigid Disk page
                            else if (page.Page == 0x04 && page.Subpage == 0)
                            {
                                var mode04 = Modes.DecodeModePage_04(page.PageResponse);
                                if (!mode04.HasValue) continue;

                                imageInfo.Cylinders = mode04.Value.Cylinders;
                                imageInfo.Heads = mode04.Value.Heads;
                                imageInfo.SectorsPerTrack =
                                    (uint) (imageInfo.Sectors / (mode04.Value.Cylinders * mode04.Value.Heads));
                            }
                            // Flexible Disk Page
                            else if (page.Page == 0x05 && page.Subpage == 0)
                            {
                                var mode05 = Modes.DecodeModePage_05(page.PageResponse);
                                if (!mode05.HasValue) continue;

                                imageInfo.Cylinders = mode05.Value.Cylinders;
                                imageInfo.Heads = mode05.Value.Heads;
                                imageInfo.SectorsPerTrack = mode05.Value.SectorsPerTrack;
                            }
                }

                if (scsiInq.HasValue)
                {
                    imageInfo.DriveManufacturer =
                        VendorString.Prettify(StringHandlers.CToString(scsiInq.Value.VendorIdentification).Trim());
                    imageInfo.DriveModel = StringHandlers.CToString(scsiInq.Value.ProductIdentification).Trim();
                    imageInfo.DriveFirmwareRevision =
                        StringHandlers.CToString(scsiInq.Value.ProductRevisionLevel).Trim();
                    imageInfo.MediaType = MediaTypeFromScsi.Get((byte) devType, imageInfo.DriveManufacturer,
                        imageInfo.DriveModel, mediumType, densityCode,
                        imageInfo.Sectors, imageInfo.SectorSize);
                }

                if (imageInfo.MediaType == MediaType.Unknown)
                    imageInfo.MediaType = devType == PeripheralDeviceTypes.OpticalDevice
                        ? MediaType.UnknownMO
                        : MediaType.GENERIC_HDD;
            }

            // It's ATA, check tags
            if (mediaTags.TryGetValue(MediaTagType.ATA_IDENTIFY, out var identifyBuf))
            {
                var ataId = Decoders.ATA.Identify.Decode(identifyBuf);
                if (ataId.HasValue)
                {
                    imageInfo.MediaType = (ushort) ataId.Value.GeneralConfiguration == 0x848A
                        ? MediaType.CompactFlash
                        : MediaType.GENERIC_HDD;

                    if (ataId.Value.Cylinders == 0 || ataId.Value.Heads == 0 || ataId.Value.SectorsPerTrack == 0)
                    {
                        imageInfo.Cylinders = ataId.Value.CurrentCylinders;
                        imageInfo.Heads = ataId.Value.CurrentHeads;
                        imageInfo.SectorsPerTrack = ataId.Value.CurrentSectorsPerTrack;
                    }
                    else
                    {
                        imageInfo.Cylinders = ataId.Value.Cylinders;
                        imageInfo.Heads = ataId.Value.Heads;
                        imageInfo.SectorsPerTrack = ataId.Value.SectorsPerTrack;
                    }
                }
            }

            switch (imageInfo.MediaType)
            {
                case MediaType.CD:
                case MediaType.CDRW:
                case MediaType.CDR:
                case MediaType.BDRE:
                case MediaType.BDROM:
                case MediaType.BDR:
                case MediaType.BDRXL:
                case MediaType.DVDPR:
                case MediaType.DVDPRDL:
                case MediaType.DVDPRW:
                case MediaType.DVDPRWDL:
                case MediaType.DVDRDL:
                case MediaType.DVDR:
                case MediaType.DVDRAM:
                case MediaType.DVDROM:
                case MediaType.DVDRWDL:
                case MediaType.DVDRW:
                case MediaType.HDDVDR:
                case MediaType.HDDVDRAM:
                case MediaType.HDDVDROM:
                case MediaType.HDDVDRW:
                case MediaType.GOD:
                case MediaType.WOD:
                case MediaType.UMD:
                case MediaType.XGD:
                case MediaType.XGD2:
                case MediaType.XGD3:
                case MediaType.PD650:
                case MediaType.PD650_WORM:
                    imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;
                    break;
                default:
                    imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                    break;
            }

            if (imageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                imageInfo.HasSessions = true;
                imageInfo.HasPartitions = true;
            }

            DicConsole.VerboseWriteLine("Raw disk image contains a disk of type {0}", imageInfo.MediaType);

            var sidecarXs = new XmlSerializer(typeof(CICMMetadataType));
            if (File.Exists(basename + "cicm.xml"))
                try
                {
                    var sr = new StreamReader(basename + "cicm.xml");
                    CicmMetadata = (CICMMetadataType) sidecarXs.Deserialize(sr);
                    sr.Close();
                }
                catch
                {
                    // Do nothing.
                }

            imageInfo.ReadableMediaTags = new List<MediaTagType>(mediaTags.Keys);

            if (!rawCompactDisc) return true;

            if (hasSubchannel)
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

            if (mode2)
            {
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
            }
            else
            {
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);
                if (!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
            }

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if (differentTrackZeroSize) throw new NotImplementedException("Not yet implemented");

            if (sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if (sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var stream = rawImageFilter.GetDataForkStream();

            uint sectorOffset = 0;
            var sectorSize = imageInfo.SectorSize;
            uint sectorSkip = 0;

            if (rawCompactDisc)
            {
                sectorOffset = 16;
                sectorSize = (uint) (mode2 ? 2336 : 2048);
                sectorSkip = (uint) (mode2 ? 0 : 288);
            }

            if (hasSubchannel) sectorSkip += 96;

            var buffer = new byte[sectorSize * length];

            var br = new BinaryReader(stream);
            br.BaseStream.Seek((long) (sectorAddress * (sectorOffset + sectorSize + sectorSkip)), SeekOrigin.Begin);
            if (sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int) (sectorSize * length));
            else
                for (var i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    var sector = br.ReadBytes((int) sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public List<Track> GetSessionTracks(Session session)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if (session.SessionSequence != 1)
                throw new ArgumentOutOfRangeException(nameof(session), "Only a single session is supported");

            var trk = new Track
            {
                TrackBytesPerSector = (int) imageInfo.SectorSize,
                TrackEndSector = imageInfo.Sectors - 1,
                TrackFilter = rawImageFilter,
                TrackFile = rawImageFilter.GetFilename(),
                TrackFileOffset = 0,
                TrackFileType = "BINARY",
                TrackRawBytesPerSector = (int) imageInfo.SectorSize,
                TrackSequence = 1,
                TrackStartSector = 0,
                TrackSubchannelType = TrackSubchannelType.None,
                TrackType = TrackType.Data,
                TrackSession = 1
            };
            var lst = new List<Track> {trk};
            return lst;
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if (session != 1)
                throw new ArgumentOutOfRangeException(nameof(session), "Only a single session is supported");

            var trk = new Track
            {
                TrackBytesPerSector = (int) imageInfo.SectorSize,
                TrackEndSector = imageInfo.Sectors - 1,
                TrackFilter = rawImageFilter,
                TrackFile = rawImageFilter.GetFilename(),
                TrackFileOffset = 0,
                TrackFileType = "BINARY",
                TrackRawBytesPerSector = (int) imageInfo.SectorSize,
                TrackSequence = 1,
                TrackStartSector = 0,
                TrackSubchannelType = TrackSubchannelType.None,
                TrackType = TrackType.Data,
                TrackSession = 1
            };
            var lst = new List<Track> {trk};
            return lst;
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if (track != 1) throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSector(sectorAddress);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if (track != 1) throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectors(sectorAddress, length);
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if (track != 1) throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectorsLong(sectorAddress, 1);
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if (track != 1) throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectorsLong(sectorAddress, length);
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc || !rawCompactDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc || !rawCompactDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if (sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if (sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip = 0;

            if (!hasSubchannel && tag == SectorTagType.CdSectorSubchannel)
                throw new ArgumentException("No tags in image for requested track", nameof(tag));

            // Requires reading sector
            if (mode2)
            {
                if (tag != SectorTagType.CdSectorSubchannel)
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");

                sectorOffset = 2352;
                sectorSize = 96;
            }
            else
            {
                switch (tag)
                {
                    case SectorTagType.CdSectorSync:
                    {
                        sectorOffset = 0;
                        sectorSize = 12;
                        sectorSkip = 2340;
                        break;
                    }

                    case SectorTagType.CdSectorHeader:
                    {
                        sectorOffset = 12;
                        sectorSize = 4;
                        sectorSkip = 2336;
                        break;
                    }

                    case SectorTagType.CdSectorSubchannel:
                    {
                        sectorOffset = 2352;
                        sectorSize = 96;
                        break;
                    }

                    case SectorTagType.CdSectorSubHeader:
                        throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                    case SectorTagType.CdSectorEcc:
                    {
                        sectorOffset = 2076;
                        sectorSize = 276;
                        sectorSkip = 0;
                        break;
                    }

                    case SectorTagType.CdSectorEccP:
                    {
                        sectorOffset = 2076;
                        sectorSize = 172;
                        sectorSkip = 104;
                        break;
                    }

                    case SectorTagType.CdSectorEccQ:
                    {
                        sectorOffset = 2248;
                        sectorSize = 104;
                        sectorSkip = 0;
                        break;
                    }

                    case SectorTagType.CdSectorEdc:
                    {
                        sectorOffset = 2064;
                        sectorSize = 4;
                        sectorSkip = 284;
                        break;
                    }

                    default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                }
            }

            var buffer = new byte[sectorSize * length];

            var stream = rawImageFilter.GetDataForkStream();
            var br = new BinaryReader(stream);
            br.BaseStream.Seek((long) (sectorAddress * (sectorOffset + sectorSize + sectorSkip)), SeekOrigin.Begin);
            if (sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int) (sectorSize * length));
            else
                for (var i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    var sector = br.ReadBytes((int) sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc || !rawCompactDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if (sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if (sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            const uint SECTOR_SIZE = 2352;
            uint sectorSkip = 0;

            if (hasSubchannel) sectorSkip += 96;

            var buffer = new byte[SECTOR_SIZE * length];

            var stream = rawImageFilter.GetDataForkStream();
            var br = new BinaryReader(stream);

            br.BaseStream.Seek((long) (sectorAddress * (SECTOR_SIZE + sectorSkip)), SeekOrigin.Begin);

            if (sectorSkip == 0) buffer = br.ReadBytes((int) (SECTOR_SIZE * length));
            else
                for (var i = 0; i < length; i++)
                {
                    var sector = br.ReadBytes((int) SECTOR_SIZE);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);

                    Array.Copy(sector, 0, buffer, i * SECTOR_SIZE, SECTOR_SIZE);
                }

            return buffer;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if (mediaTags.TryGetValue(tag, out var data)) return data;

            throw new FeatureNotPresentImageException("Requested tag is not present in image");
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc || !rawCompactDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if (track != 1) throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if (imageInfo.XmlMediaType != XmlMediaType.OpticalDisc || !rawCompactDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if (track != 1) throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectorsTag(sectorAddress, length, tag);
        }
    }
}