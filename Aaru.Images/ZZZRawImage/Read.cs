﻿// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Helpers;
using Schemas;
using DMI = Aaru.Decoders.Xbox.DMI;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;
using Session = Aaru.CommonTypes.Structs.Session;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages
{
    public sealed partial class ZZZRawImage
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            _extension = Path.GetExtension(imageFilter.GetFilename())?.ToLower();

            switch(_extension)
            {
                case ".1kn":
                    _imageInfo.SectorSize = 1024;

                    break;
                case ".2kn":
                    _imageInfo.SectorSize = 2048;

                    break;
                case ".4kn":
                    _imageInfo.SectorSize = 4096;

                    break;
                case ".8kn":
                    _imageInfo.SectorSize = 8192;

                    break;
                case ".16kn":
                    _imageInfo.SectorSize = 16384;

                    break;
                case ".32kn":
                    _imageInfo.SectorSize = 32768;

                    break;
                case ".64kn":
                    _imageInfo.SectorSize = 65536;

                    break;
                case ".512":
                case ".512e":
                    _imageInfo.SectorSize = 512;

                    break;
                case ".128":
                    _imageInfo.SectorSize = 128;

                    break;
                case ".256":
                    _imageInfo.SectorSize = 256;

                    break;
                case ".2352":
                    _imageInfo.SectorSize = 2352;

                    break;
                case ".2448":
                    _imageInfo.SectorSize = 2448;

                    break;

                case ".iso" when imageFilter.GetDataForkLength() % 2048 == 0:
                    _imageInfo.SectorSize = 2048;

                    break;
                case ".toast" when imageFilter.GetDataForkLength() % 2048 == 0:
                    _imageInfo.SectorSize = 2048;

                    break;
                case ".toast" when imageFilter.GetDataForkLength() % 2336 == 0:
                    _imageInfo.SectorSize = 2336;

                    break;
                case ".toast" when imageFilter.GetDataForkLength() % 2352 == 0:
                    _imageInfo.SectorSize = 2352;

                    break;
                case ".d81" when imageFilter.GetDataForkLength() == 819200:
                    _imageInfo.SectorSize = 256;

                    break;
                default:
                    if((_extension == ".adf" || _extension == ".adl" || _extension == ".ssd" || _extension == ".dsd") &&
                       (imageFilter.GetDataForkLength() == 163840 || imageFilter.GetDataForkLength() == 327680 ||
                        imageFilter.GetDataForkLength() == 655360))
                        _imageInfo.SectorSize = 256;
                    else if((_extension == ".adf" || _extension == ".adl") &&
                            imageFilter.GetDataForkLength() == 819200)
                        _imageInfo.SectorSize = 1024;
                    else
                        switch(imageFilter.GetDataForkLength())
                        {
                            case 242944:
                            case 256256:
                            case 495872:
                            case 92160:
                            case 133120:
                                _imageInfo.SectorSize = 128;

                                break;
                            case 116480:
                            case 287488:  // T0S0 = 128bps
                            case 988416:  // T0S0 = 128bps
                            case 995072:  // T0S0 = 128bps, T0S1 = 256bps
                            case 1021696: // T0S0 = 128bps, T0S1 = 256bps
                            case 232960:
                            case 143360:
                            case 286720:
                            case 512512:
                            case 102400:
                            case 204800:
                            case 655360:
                            case 80384:  // T0S0 = 128bps
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

                                _imageInfo.SectorSize = 256;

                                break;
                            case 81664:
                                _imageInfo.SectorSize = 319;

                                break;
                            case 306432:  // T0S0 = 128bps
                            case 1146624: // T0S0 = 128bps, T0S1 = 256bps
                            case 1177344: // T0S0 = 128bps, T0S1 = 256bps
                                _imageInfo.SectorSize = 512;

                                break;
                            case 1222400: // T0S0 = 128bps, T0S1 = 256bps
                            case 1304320: // T0S0 = 128bps, T0S1 = 256bps
                            case 1255168: // T0S0 = 128bps, T0S1 = 256bps
                            case 1261568:
                            case 1638400:
                                _imageInfo.SectorSize = 1024;

                                break;
                            case 35002122240:
                                _imageInfo.SectorSize = 2048;

                                break;
                            default:
                                _imageInfo.SectorSize = 512;

                                break;
                        }

                    break;
            }

            _imageInfo.ImageSize            = (ulong)imageFilter.GetDataForkLength();
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            _differentTrackZeroSize         = false;
            _rawImageFilter                 = imageFilter;

            switch(imageFilter.GetDataForkLength())
            {
                case 242944:
                    _imageInfo.Sectors = 1898;

                    break;
                case 256256:
                    _imageInfo.Sectors = 2002;

                    break;
                case 495872:
                    _imageInfo.Sectors = 3874;

                    break;
                case 116480:
                    _imageInfo.Sectors = 455;

                    break;
                case 287488: // T0S0 = 128bps
                    _imageInfo.Sectors      = 1136;
                    _differentTrackZeroSize = true;

                    break;
                case 988416: // T0S0 = 128bps
                    _imageInfo.Sectors      = 3874;
                    _differentTrackZeroSize = true;

                    break;
                case 995072: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.Sectors      = 3900;
                    _differentTrackZeroSize = true;

                    break;
                case 1021696: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.Sectors      = 4004;
                    _differentTrackZeroSize = true;

                    break;
                case 81664:
                    _imageInfo.Sectors = 256;

                    break;
                case 306432: // T0S0 = 128bps
                    _imageInfo.Sectors      = 618;
                    _differentTrackZeroSize = true;

                    break;
                case 1146624: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.Sectors      = 2272;
                    _differentTrackZeroSize = true;

                    break;
                case 1177344: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.Sectors      = 2332;
                    _differentTrackZeroSize = true;

                    break;
                case 1222400: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.Sectors      = 1236;
                    _differentTrackZeroSize = true;

                    break;
                case 1304320: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.Sectors      = 1316;
                    _differentTrackZeroSize = true;

                    break;
                case 1255168: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.Sectors      = 1268;
                    _differentTrackZeroSize = true;

                    break;
                case 80384: // T0S0 = 128bps
                    _imageInfo.Sectors      = 322;
                    _differentTrackZeroSize = true;

                    break;
                case 325632: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.Sectors      = 1280;
                    _differentTrackZeroSize = true;

                    break;
                case 653312: // T0S0 = 128bps, T0S1 = 256bps
                    _imageInfo.Sectors      = 2560;
                    _differentTrackZeroSize = true;

                    break;
                case 1880064: // IBM XDF, 3,5", real number of sectors
                    _imageInfo.Sectors      = 670;
                    _imageInfo.SectorSize   = 8192; // Biggest sector size
                    _differentTrackZeroSize = true;

                    break;
                case 175531:
                    _imageInfo.Sectors = 683;

                    break;
                case 197375:
                    _imageInfo.Sectors = 768;

                    break;
                case 351062:
                    _imageInfo.Sectors = 1366;

                    break;
                case 822400:
                    _imageInfo.Sectors = 3200;

                    break;
                default:
                    _imageInfo.Sectors = _imageInfo.ImageSize / _imageInfo.SectorSize;

                    break;
            }

            _imageInfo.MediaType = CalculateDiskType();

            if(_imageInfo.ImageSize % 2352 == 0 ||
               _imageInfo.ImageSize % 2448 == 0)
            {
                byte[] sync   = new byte[12];
                byte[] header = new byte[4];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(sync, 0, 12);
                stream.Read(header, 0, 4);

                if(_cdSync.SequenceEqual(sync))
                {
                    _rawCompactDisc       = true;
                    _hasSubchannel        = _imageInfo.ImageSize % 2448 == 0;
                    _imageInfo.Sectors    = _imageInfo.ImageSize / (ulong)(_hasSubchannel ? 2448 : 2352);
                    _imageInfo.MediaType  = MediaType.CD;
                    _mode2                = header[3] == 0x02;
                    _imageInfo.SectorSize = (uint)(_mode2 ? 2336 : 2048);
                }
            }

            // Sharp X68000 SASI hard disks
            if(_extension == ".hdf")
                if(_imageInfo.ImageSize % 256 == 0)
                {
                    _imageInfo.SectorSize = 256;
                    _imageInfo.Sectors    = _imageInfo.ImageSize / _imageInfo.SectorSize;
                    _imageInfo.MediaType  = MediaType.GENERIC_HDD;
                }

            // Search for known tags
            string basename = imageFilter.GetBasePath();
            basename = basename.Substring(0, basename.Length - _extension.Length);

            _mediaTags = new Dictionary<MediaTagType, byte[]>();

            foreach((MediaTagType tag, string name) sidecar in _readWriteSidecars)
                try
                {
                    var     filters = new FiltersList();
                    IFilter filter  = filters.GetFilter(basename + sidecar.name);

                    if(filter?.IsOpened() != true)
                        continue;

                    AaruConsole.DebugWriteLine("ZZZRawImage Plugin", "Found media tag {0}", sidecar.tag);
                    byte[] data = new byte[filter.GetDataForkLength()];
                    filter.GetDataForkStream().Read(data, 0, data.Length);
                    _mediaTags.Add(sidecar.tag, data);
                }
                catch(IOException) {}

            // If there are INQUIRY and IDENTIFY tags, it's ATAPI
            if(_mediaTags.ContainsKey(MediaTagType.SCSI_INQUIRY))
                if(_mediaTags.TryGetValue(MediaTagType.ATA_IDENTIFY, out byte[] tag))
                {
                    _mediaTags.Remove(MediaTagType.ATA_IDENTIFY);
                    _mediaTags.Add(MediaTagType.ATAPI_IDENTIFY, tag);
                }

            // It is a blu-ray
            if(_mediaTags.ContainsKey(MediaTagType.BD_DI))
            {
                _imageInfo.MediaType = MediaType.BDROM;

                if(_mediaTags.TryGetValue(MediaTagType.DVD_BCA, out byte[] bca))
                {
                    _mediaTags.Remove(MediaTagType.DVD_BCA);
                    _mediaTags.Add(MediaTagType.BD_BCA, bca);
                }

                if(_mediaTags.TryGetValue(MediaTagType.DVDRAM_DDS, out byte[] dds))
                {
                    _imageInfo.MediaType = MediaType.BDRE;
                    _mediaTags.Remove(MediaTagType.DVDRAM_DDS);
                    _mediaTags.Add(MediaTagType.BD_DDS, dds);
                }

                if(_mediaTags.TryGetValue(MediaTagType.DVDRAM_SpareArea, out byte[] sai))
                {
                    _imageInfo.MediaType = MediaType.BDRE;
                    _mediaTags.Remove(MediaTagType.DVDRAM_SpareArea);
                    _mediaTags.Add(MediaTagType.BD_SpareArea, sai);
                }
            }

            // It is a DVD
            if(_mediaTags.TryGetValue(MediaTagType.DVD_PFI, out byte[] pfi))
            {
                PFI.PhysicalFormatInformation decPfi = PFI.Decode(pfi, _imageInfo.MediaType).Value;

                switch(decPfi.DiskCategory)
                {
                    case DiskCategory.DVDPR:
                        _imageInfo.MediaType = MediaType.DVDPR;

                        break;
                    case DiskCategory.DVDPRDL:
                        _imageInfo.MediaType = MediaType.DVDPRDL;

                        break;
                    case DiskCategory.DVDPRW:
                        _imageInfo.MediaType = MediaType.DVDPRW;

                        break;
                    case DiskCategory.DVDPRWDL:
                        _imageInfo.MediaType = MediaType.DVDPRWDL;

                        break;
                    case DiskCategory.DVDR:
                        _imageInfo.MediaType = decPfi.PartVersion >= 6 ? MediaType.DVDRDL : MediaType.DVDR;

                        break;
                    case DiskCategory.DVDRAM:
                        _imageInfo.MediaType = MediaType.DVDRAM;

                        break;
                    default:
                        _imageInfo.MediaType = MediaType.DVDROM;

                        break;
                    case DiskCategory.DVDRW:
                        _imageInfo.MediaType = decPfi.PartVersion >= 15 ? MediaType.DVDRWDL : MediaType.DVDRW;

                        break;
                    case DiskCategory.HDDVDR:
                        _imageInfo.MediaType = MediaType.HDDVDR;

                        break;
                    case DiskCategory.HDDVDRAM:
                        _imageInfo.MediaType = MediaType.HDDVDRAM;

                        break;
                    case DiskCategory.HDDVDROM:
                        _imageInfo.MediaType = MediaType.HDDVDROM;

                        break;
                    case DiskCategory.HDDVDRW:
                        _imageInfo.MediaType = MediaType.HDDVDRW;

                        break;
                    case DiskCategory.Nintendo:
                        _imageInfo.MediaType = decPfi.DiscSize == DVDSize.Eighty ? MediaType.GOD : MediaType.WOD;

                        break;
                    case DiskCategory.UMD:
                        _imageInfo.MediaType = MediaType.UMD;

                        break;
                }

                if((_imageInfo.MediaType == MediaType.DVDR || _imageInfo.MediaType == MediaType.DVDRW ||
                    _imageInfo.MediaType == MediaType.HDDVDR) &&
                   _mediaTags.TryGetValue(MediaTagType.DVD_MediaIdentifier, out byte[] mid))
                {
                    _mediaTags.Remove(MediaTagType.DVD_MediaIdentifier);
                    _mediaTags.Add(MediaTagType.DVDR_MediaIdentifier, mid);
                }

                // Check for Xbox
                if(_mediaTags.TryGetValue(MediaTagType.DVD_DMI, out byte[] dmi))
                    if(DMI.IsXbox(dmi) ||
                       DMI.IsXbox360(dmi))
                        if(DMI.IsXbox(dmi))
                        {
                            _imageInfo.MediaType = MediaType.XGD;
                        }
                        else if(DMI.IsXbox360(dmi))
                        {
                            _imageInfo.MediaType = MediaType.XGD2;

                            // All XGD3 all have the same number of blocks
                            if(_imageInfo.Sectors == 25063   || // Locked (or non compatible drive)
                               _imageInfo.Sectors == 4229664 || // Xtreme unlock
                               _imageInfo.Sectors == 4246304)   // Wxripper unlock
                                _imageInfo.MediaType = MediaType.XGD3;
                        }
            }

            // It's MultiMediaCard or SecureDigital
            if(_mediaTags.ContainsKey(MediaTagType.SD_CID) ||
               _mediaTags.ContainsKey(MediaTagType.SD_CSD) ||
               _mediaTags.ContainsKey(MediaTagType.SD_OCR))
            {
                _imageInfo.MediaType = MediaType.SecureDigital;

                if(_mediaTags.ContainsKey(MediaTagType.MMC_ExtendedCSD) ||
                   !_mediaTags.ContainsKey(MediaTagType.SD_SCR))
                {
                    _imageInfo.MediaType = MediaType.MMC;

                    if(_mediaTags.TryGetValue(MediaTagType.SD_CID, out byte[] cid))
                    {
                        _mediaTags.Remove(MediaTagType.SD_CID);
                        _mediaTags.Add(MediaTagType.MMC_CID, cid);
                    }

                    if(_mediaTags.TryGetValue(MediaTagType.SD_CSD, out byte[] csd))
                    {
                        _mediaTags.Remove(MediaTagType.SD_CSD);
                        _mediaTags.Add(MediaTagType.MMC_CSD, csd);
                    }

                    if(_mediaTags.TryGetValue(MediaTagType.SD_OCR, out byte[] ocr))
                    {
                        _mediaTags.Remove(MediaTagType.SD_OCR);
                        _mediaTags.Add(MediaTagType.MMC_OCR, ocr);
                    }
                }
            }

            // It's a compact disc
            if(_mediaTags.ContainsKey(MediaTagType.CD_FullTOC))
            {
                _imageInfo.MediaType = _imageInfo.Sectors > 360000 ? MediaType.DDCD : MediaType.CD;

                // Only CD-R and CD-RW have ATIP
                if(_mediaTags.TryGetValue(MediaTagType.CD_ATIP, out byte[] atipBuf))
                {
                    ATIP.CDATIP atip = ATIP.Decode(atipBuf);

                    if(atip != null)
                        _imageInfo.MediaType = atip.DiscType ? MediaType.CDRW : MediaType.CDR;
                }

                if(_mediaTags.TryGetValue(MediaTagType.Floppy_LeadOut, out byte[] leadout))
                {
                    _mediaTags.Remove(MediaTagType.Floppy_LeadOut);
                    _mediaTags.Add(MediaTagType.CD_LeadOut, leadout);
                }
            }

            switch(_imageInfo.MediaType)
            {
                case MediaType.ACORN_35_DS_DD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 5;

                    break;
                case MediaType.ACORN_35_DS_HD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.ACORN_525_DS_DD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case MediaType.ACORN_525_SS_DD_40:
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case MediaType.ACORN_525_SS_DD_80:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case MediaType.ACORN_525_SS_SD_40:
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.ACORN_525_SS_SD_80:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.Apple32DS:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 13;

                    break;
                case MediaType.Apple32SS:
                    _imageInfo.Cylinders       = 36;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 13;

                    break;
                case MediaType.Apple33DS:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case MediaType.Apple33SS:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case MediaType.AppleSonyDS:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.AppleSonySS:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.ATARI_35_DS_DD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.ATARI_35_DS_DD_11:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 11;

                    break;
                case MediaType.ATARI_35_SS_DD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.ATARI_35_SS_DD_11:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 11;

                    break;
                case MediaType.ATARI_525_ED:
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 26;

                    break;
                case MediaType.ATARI_525_SD:
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 18;

                    break;
                case MediaType.CBM_35_DD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.CBM_AMIGA_35_DD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 11;

                    break;
                case MediaType.CBM_AMIGA_35_HD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 22;

                    break;
                case MediaType.DMF:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 21;

                    break;
                case MediaType.DOS_35_DS_DD_9:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 9;

                    break;
                case MediaType.Apricot_35:
                    _imageInfo.Cylinders       = 70;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 9;

                    break;
                case MediaType.DOS_35_ED:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 36;

                    break;
                case MediaType.DOS_35_HD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 18;

                    break;
                case MediaType.DOS_35_SS_DD_9:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 9;

                    break;
                case MediaType.DOS_525_DS_DD_8:
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 8;

                    break;
                case MediaType.DOS_525_DS_DD_9:
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 9;

                    break;
                case MediaType.DOS_525_HD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 15;

                    break;
                case MediaType.DOS_525_SS_DD_8:
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 8;

                    break;
                case MediaType.DOS_525_SS_DD_9:
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 9;

                    break;
                case MediaType.ECMA_54:
                    _imageInfo.Cylinders       = 77;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 26;

                    break;
                case MediaType.ECMA_59:
                    _imageInfo.Cylinders       = 77;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 26;

                    break;
                case MediaType.ECMA_66:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 9;

                    break;
                case MediaType.ECMA_69_8:
                    _imageInfo.Cylinders       = 77;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 8;

                    break;
                case MediaType.ECMA_70:
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case MediaType.ECMA_78:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case MediaType.ECMA_99_15:
                    _imageInfo.Cylinders       = 77;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 15;

                    break;
                case MediaType.ECMA_99_26:
                    _imageInfo.Cylinders       = 77;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 26;

                    break;
                case MediaType.ECMA_99_8:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 8;

                    break;
                case MediaType.FDFORMAT_35_DD:
                    _imageInfo.Cylinders       = 82;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.FDFORMAT_35_HD:
                    _imageInfo.Cylinders       = 82;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 21;

                    break;
                case MediaType.FDFORMAT_525_HD:
                    _imageInfo.Cylinders       = 82;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 17;

                    break;
                case MediaType.IBM23FD:
                    _imageInfo.Cylinders       = 32;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 8;

                    break;
                case MediaType.IBM33FD_128:
                    _imageInfo.Cylinders       = 73;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 26;

                    break;
                case MediaType.IBM33FD_256:
                    _imageInfo.Cylinders       = 74;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 15;

                    break;
                case MediaType.IBM33FD_512:
                    _imageInfo.Cylinders       = 74;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 8;

                    break;
                case MediaType.IBM43FD_128:
                    _imageInfo.Cylinders       = 74;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 26;

                    break;
                case MediaType.IBM43FD_256:
                    _imageInfo.Cylinders       = 74;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 15;

                    break;
                case MediaType.IBM53FD_1024:
                    _imageInfo.Cylinders       = 74;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 8;

                    break;
                case MediaType.IBM53FD_256:
                    _imageInfo.Cylinders       = 74;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 26;

                    break;
                case MediaType.IBM53FD_512:
                    _imageInfo.Cylinders       = 74;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 15;

                    break;
                case MediaType.NEC_35_TD:
                    _imageInfo.Cylinders       = 240;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 38;

                    break;
                case MediaType.NEC_525_HD:
                    _imageInfo.Cylinders       = 77;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 8;

                    break;
                case MediaType.XDF_35:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 23;

                    break;

                // Following ones are what the device itself report, not the physical geometry
                case MediaType.Jaz:
                    _imageInfo.Cylinders       = 1021;
                    _imageInfo.Heads           = 64;
                    _imageInfo.SectorsPerTrack = 32;

                    break;
                case MediaType.PocketZip:
                    _imageInfo.Cylinders       = 154;
                    _imageInfo.Heads           = 16;
                    _imageInfo.SectorsPerTrack = 32;

                    break;
                case MediaType.LS120:
                    _imageInfo.Cylinders       = 963;
                    _imageInfo.Heads           = 8;
                    _imageInfo.SectorsPerTrack = 32;

                    break;
                case MediaType.LS240:
                    _imageInfo.Cylinders       = 262;
                    _imageInfo.Heads           = 32;
                    _imageInfo.SectorsPerTrack = 56;

                    break;
                case MediaType.FD32MB:
                    _imageInfo.Cylinders       = 1024;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 32;

                    break;
                case MediaType.ZIP100:
                    _imageInfo.Cylinders       = 96;
                    _imageInfo.Heads           = 64;
                    _imageInfo.SectorsPerTrack = 32;

                    break;
                case MediaType.ZIP250:
                    _imageInfo.Cylinders       = 239;
                    _imageInfo.Heads           = 64;
                    _imageInfo.SectorsPerTrack = 32;

                    break;
                case MediaType.MetaFloppy_Mod_I:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case MediaType.MetaFloppy_Mod_II:
                    _imageInfo.Cylinders       = 77;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                default:
                    _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
                    _imageInfo.Heads           = 16;
                    _imageInfo.SectorsPerTrack = 63;

                    break;
            }

            // It's SCSI, check tags
            if(_mediaTags.ContainsKey(MediaTagType.SCSI_INQUIRY))
            {
                PeripheralDeviceTypes devType = PeripheralDeviceTypes.DirectAccess;
                Inquiry?              scsiInq = null;

                if(_mediaTags.TryGetValue(MediaTagType.SCSI_INQUIRY, out byte[] inq))
                {
                    scsiInq = Inquiry.Decode(inq);
                    devType = (PeripheralDeviceTypes)(inq[0] & 0x1F);
                }

                Modes.DecodedMode? decMode = null;

                if(_mediaTags.TryGetValue(MediaTagType.SCSI_MODESENSE_6, out byte[] mode6))
                    decMode = Modes.DecodeMode6(mode6, devType);
                else if(_mediaTags.TryGetValue(MediaTagType.SCSI_MODESENSE_10, out byte[] mode10))
                    decMode = Modes.DecodeMode10(mode10, devType);

                byte mediumType  = 0;
                byte densityCode = 0;

                if(decMode.HasValue)
                {
                    mediumType = (byte)decMode.Value.Header.MediumType;

                    if(decMode?.Header.BlockDescriptors?.Length > 0)
                        densityCode = (byte)decMode.Value.Header.BlockDescriptors[0].Density;

                    if(decMode.Value.Pages != null)
                        foreach(Modes.ModePage page in decMode.Value.Pages)

                            // CD-ROM page
                            if(page.Page    == 0x2A &&
                               page.Subpage == 0)
                            {
                                if(_mediaTags.ContainsKey(MediaTagType.SCSI_MODEPAGE_2A))
                                    _mediaTags.Remove(MediaTagType.SCSI_MODEPAGE_2A);

                                _mediaTags.Add(MediaTagType.SCSI_MODEPAGE_2A, page.PageResponse);
                            }

                            // Rigid Disk page
                            else if(page.Page    == 0x04 &&
                                    page.Subpage == 0)
                            {
                                Modes.ModePage_04? mode04 = Modes.DecodeModePage_04(page.PageResponse);

                                if(!mode04.HasValue)
                                    continue;

                                _imageInfo.Cylinders = mode04.Value.Cylinders;
                                _imageInfo.Heads     = mode04.Value.Heads;

                                _imageInfo.SectorsPerTrack =
                                    (uint)(_imageInfo.Sectors / (mode04.Value.Cylinders * mode04.Value.Heads));
                            }

                            // Flexible Disk Page
                            else if(page.Page    == 0x05 &&
                                    page.Subpage == 0)
                            {
                                Modes.ModePage_05? mode05 = Modes.DecodeModePage_05(page.PageResponse);

                                if(!mode05.HasValue)
                                    continue;

                                _imageInfo.Cylinders       = mode05.Value.Cylinders;
                                _imageInfo.Heads           = mode05.Value.Heads;
                                _imageInfo.SectorsPerTrack = mode05.Value.SectorsPerTrack;
                            }
                }

                if(scsiInq.HasValue)
                {
                    _imageInfo.DriveManufacturer =
                        VendorString.Prettify(StringHandlers.CToString(scsiInq.Value.VendorIdentification).Trim());

                    _imageInfo.DriveModel = StringHandlers.CToString(scsiInq.Value.ProductIdentification).Trim();

                    _imageInfo.DriveFirmwareRevision =
                        StringHandlers.CToString(scsiInq.Value.ProductRevisionLevel).Trim();

                    _imageInfo.MediaType = MediaTypeFromDevice.GetFromScsi((byte)devType, _imageInfo.DriveManufacturer,
                                                                           _imageInfo.DriveModel, mediumType,
                                                                           densityCode, _imageInfo.Sectors,
                                                                           _imageInfo.SectorSize,
                                                                           _mediaTags.ContainsKey(MediaTagType.
                                                                               USB_Descriptors),
                                                                           _rawCompactDisc);
                }

                if(_imageInfo.MediaType == MediaType.Unknown)
                    _imageInfo.MediaType = devType == PeripheralDeviceTypes.OpticalDevice ? MediaType.UnknownMO
                                               : MediaType.GENERIC_HDD;
            }

            // It's ATA, check tags
            if(_mediaTags.TryGetValue(MediaTagType.ATA_IDENTIFY, out byte[] identifyBuf))
            {
                Identify.IdentifyDevice? ataId = CommonTypes.Structs.Devices.ATA.Identify.Decode(identifyBuf);

                if(ataId.HasValue)
                {
                    _imageInfo.MediaType = (ushort)ataId.Value.GeneralConfiguration == 0x848A ? MediaType.CompactFlash
                                               : MediaType.GENERIC_HDD;

                    if(ataId.Value.Cylinders       == 0 ||
                       ataId.Value.Heads           == 0 ||
                       ataId.Value.SectorsPerTrack == 0)
                    {
                        _imageInfo.Cylinders       = ataId.Value.CurrentCylinders;
                        _imageInfo.Heads           = ataId.Value.CurrentHeads;
                        _imageInfo.SectorsPerTrack = ataId.Value.CurrentSectorsPerTrack;
                    }
                    else
                    {
                        _imageInfo.Cylinders       = ataId.Value.Cylinders;
                        _imageInfo.Heads           = ataId.Value.Heads;
                        _imageInfo.SectorsPerTrack = ataId.Value.SectorsPerTrack;
                    }
                }
            }

            switch(_imageInfo.MediaType)
            {
                case MediaType.CD:
                case MediaType.CDRW:
                case MediaType.CDR:
                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                    goto case MediaType.BDRE;
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
                    _imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                    break;
                default:
                    _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

                    break;
            }

            if(_imageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                _imageInfo.HasSessions   = true;
                _imageInfo.HasPartitions = true;
            }

            AaruConsole.VerboseWriteLine("Raw disk image contains a disk of type {0}", _imageInfo.MediaType);

            var sidecarXs = new XmlSerializer(typeof(CICMMetadataType));

            if(File.Exists(basename + "cicm.xml"))
                try
                {
                    var sr = new StreamReader(basename + "cicm.xml");
                    CicmMetadata = (CICMMetadataType)sidecarXs.Deserialize(sr);
                    sr.Close();
                }
                catch
                {
                    // Do nothing.
                }

            _imageInfo.ReadableMediaTags = new List<MediaTagType>(_mediaTags.Keys);

            if(!_rawCompactDisc)
                return true;

            if(_hasSubchannel)
                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

            if(_mode2)
            {
                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
            }
            else
            {
                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
            }

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(_differentTrackZeroSize)
                throw new NotImplementedException("Not yet implemented");

            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            Stream stream = _rawImageFilter.GetDataForkStream();

            uint sectorOffset = 0;
            uint sectorSize   = _imageInfo.SectorSize;
            uint sectorSkip   = 0;

            if(_rawCompactDisc)
            {
                sectorOffset = (uint)(_mode2 ? 0 : 16);
                sectorSize   = (uint)(_mode2 ? 2352 : 2048);
                sectorSkip   = (uint)(_mode2 ? 0 : 288);
            }

            if(_hasSubchannel)
                sectorSkip += 96;

            byte[] buffer = new byte[sectorSize * length];

            var br = new BinaryReader(stream);
            br.BaseStream.Seek((long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)), SeekOrigin.Begin);

            if(_mode2)
            {
                var mode2Ms = new MemoryStream((int)(sectorSize * length));

                buffer = br.ReadBytes((int)((sectorSize + sectorSkip) * length));

                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    Array.Copy(buffer, (sectorSize + sectorSkip) * i, sector, 0, sectorSize);
                    sector = Sector.GetUserDataFromMode2(sector);
                    mode2Ms.Write(sector, 0, sector.Length);
                }

                buffer = mode2Ms.ToArray();
            }
            else if(sectorOffset == 0 &&
                    sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(Session session)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(session.SessionSequence != 1)
                throw new ArgumentOutOfRangeException(nameof(session), "Only a single session is supported");

            var trk = new Track
            {
                TrackBytesPerSector    = (int)_imageInfo.SectorSize,
                TrackEndSector         = _imageInfo.Sectors - 1,
                TrackFilter            = _rawImageFilter,
                TrackFile              = _rawImageFilter.GetFilename(),
                TrackFileOffset        = 0,
                TrackFileType          = "BINARY",
                TrackRawBytesPerSector = (int)_imageInfo.SectorSize,
                TrackSequence          = 1,
                TrackStartSector       = 0,
                TrackSubchannelType    = TrackSubchannelType.None,
                TrackType              = TrackType.Data,
                TrackSession           = 1
            };

            List<Track> lst = new List<Track>
            {
                trk
            };

            return lst;
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(ushort session)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(session != 1)
                throw new ArgumentOutOfRangeException(nameof(session), "Only a single session is supported");

            var trk = new Track
            {
                TrackBytesPerSector    = (int)_imageInfo.SectorSize,
                TrackEndSector         = _imageInfo.Sectors - 1,
                TrackFilter            = _rawImageFilter,
                TrackFile              = _rawImageFilter.GetFilename(),
                TrackFileOffset        = 0,
                TrackFileType          = "BINARY",
                TrackRawBytesPerSector = (int)_imageInfo.SectorSize,
                TrackSequence          = 1,
                TrackStartSector       = 0,
                TrackSubchannelType    = TrackSubchannelType.None,
                TrackType              = TrackType.Data,
                TrackSession           = 1
            };

            List<Track> lst = new List<Track>
            {
                trk
            };

            return lst;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(track != 1)
                throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSector(sectorAddress);
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(track != 1)
                throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectors(sectorAddress, length);
        }

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(track != 1)
                throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectorsLong(sectorAddress, 1);
        }

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(track != 1)
                throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectorsLong(sectorAddress, length);
        }

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc ||
               (!_rawCompactDisc && tag != SectorTagType.CdTrackFlags))
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc ||
               (!_rawCompactDisc && tag != SectorTagType.CdTrackFlags))
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(tag == SectorTagType.CdTrackFlags)
                return new byte[]
                {
                    4
                };

            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip = 0;

            if(!_hasSubchannel &&
               tag == SectorTagType.CdSectorSubchannel)
                throw new ArgumentException("No tags in image for requested track", nameof(tag));

            // Requires reading sector
            if(_mode2)
            {
                if(tag != SectorTagType.CdSectorSubchannel)
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");

                sectorOffset = 2352;
                sectorSize   = 96;
            }
            else
            {
                switch(tag)
                {
                    case SectorTagType.CdSectorSync:
                    {
                        sectorOffset = 0;
                        sectorSize   = 12;
                        sectorSkip   = 2340;

                        break;
                    }

                    case SectorTagType.CdSectorHeader:
                    {
                        sectorOffset = 12;
                        sectorSize   = 4;
                        sectorSkip   = 2336;

                        break;
                    }

                    case SectorTagType.CdSectorSubchannel:
                    {
                        sectorOffset = 2352;
                        sectorSize   = 96;

                        break;
                    }

                    case SectorTagType.CdSectorSubHeader:
                        throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                    case SectorTagType.CdSectorEcc:
                    {
                        sectorOffset = 2076;
                        sectorSize   = 276;
                        sectorSkip   = 0;

                        break;
                    }

                    case SectorTagType.CdSectorEccP:
                    {
                        sectorOffset = 2076;
                        sectorSize   = 172;
                        sectorSkip   = 104;

                        break;
                    }

                    case SectorTagType.CdSectorEccQ:
                    {
                        sectorOffset = 2248;
                        sectorSize   = 104;
                        sectorSkip   = 0;

                        break;
                    }

                    case SectorTagType.CdSectorEdc:
                    {
                        sectorOffset = 2064;
                        sectorSize   = 4;
                        sectorSkip   = 284;

                        break;
                    }

                    default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                }
            }

            byte[] buffer = new byte[sectorSize * length];

            Stream stream = _rawImageFilter.GetDataForkStream();
            var    br     = new BinaryReader(stream);
            br.BaseStream.Seek((long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)), SeekOrigin.Begin);

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc ||
               !_rawCompactDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            const uint sectorSize = 2352;
            uint       sectorSkip = 0;

            if(_hasSubchannel)
                sectorSkip += 96;

            byte[] buffer = new byte[sectorSize * length];

            Stream stream = _rawImageFilter.GetDataForkStream();
            var    br     = new BinaryReader(stream);

            br.BaseStream.Seek((long)(sectorAddress * (sectorSize + sectorSkip)), SeekOrigin.Begin);

            if(sectorSkip == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);

                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(_mediaTags.TryGetValue(tag, out byte[] data))
                return data;

            throw new FeatureNotPresentImageException("Requested tag is not present in image");
        }

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc ||
               !_rawCompactDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(track != 1)
                throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc ||
               !_rawCompactDisc)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(track != 1)
                throw new ArgumentOutOfRangeException(nameof(track), "Only a single track is supported");

            return ReadSectorsTag(sectorAddress, length, tag);
        }
    }
}