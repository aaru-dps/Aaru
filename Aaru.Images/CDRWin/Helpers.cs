// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for CDRWin cuesheets (cue/bin).
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    public sealed partial class CdrWin
    {
        static int CdrWinMsfToLba(string msf)
        {
            string[] msfElements = msf.Split(':');
            int      minute      = int.Parse(msfElements[0]);
            int      second      = int.Parse(msfElements[1]);
            int      frame       = int.Parse(msfElements[2]);

            int sectors = (minute * 60 * 75) + (second * 75) + frame;

            return sectors;
        }

        static ushort CdrWinTrackTypeToBytesPerSector(string trackType)
        {
            switch(trackType)
            {
                case CDRWIN_TRACK_TYPE_MODE1:
                case CDRWIN_TRACK_TYPE_MODE2_FORM1: return 2048;
                case CDRWIN_TRACK_TYPE_MODE2_FORM2: return 2324;
                case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                case CDRWIN_TRACK_TYPE_CDI: return 2336;
                case CDRWIN_TRACK_TYPE_AUDIO:
                case CDRWIN_TRACK_TYPE_MODE1_RAW:
                case CDRWIN_TRACK_TYPE_MODE2_RAW:
                case CDRWIN_TRACK_TYPE_CDI_RAW: return 2352;
                case CDRWIN_TRACK_TYPE_CDG: return 2448;
                default:                    return 0;
            }
        }

        static ushort CdrWinTrackTypeToCookedBytesPerSector(string trackType)
        {
            switch(trackType)
            {
                case CDRWIN_TRACK_TYPE_MODE1:
                case CDRWIN_TRACK_TYPE_MODE2_FORM1:
                case CDRWIN_TRACK_TYPE_MODE1_RAW: return 2048;
                case CDRWIN_TRACK_TYPE_MODE2_FORM2: return 2324;
                case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                case CDRWIN_TRACK_TYPE_CDI:
                case CDRWIN_TRACK_TYPE_MODE2_RAW:
                case CDRWIN_TRACK_TYPE_CDI_RAW: return 2336;
                case CDRWIN_TRACK_TYPE_CDG:
                case CDRWIN_TRACK_TYPE_AUDIO: return 2352;
                default: return 0;
            }
        }

        static TrackType CdrWinTrackTypeToTrackType(string trackType)
        {
            switch(trackType)
            {
                case CDRWIN_TRACK_TYPE_MODE1:
                case CDRWIN_TRACK_TYPE_MODE1_RAW: return TrackType.CdMode1;
                case CDRWIN_TRACK_TYPE_MODE2_FORM1: return TrackType.CdMode2Form1;
                case CDRWIN_TRACK_TYPE_MODE2_FORM2: return TrackType.CdMode2Form2;
                case CDRWIN_TRACK_TYPE_CDI_RAW:
                case CDRWIN_TRACK_TYPE_CDI:
                case CDRWIN_TRACK_TYPE_MODE2_RAW:
                case CDRWIN_TRACK_TYPE_MODE2_FORMLESS: return TrackType.CdMode2Formless;
                case CDRWIN_TRACK_TYPE_AUDIO:
                case CDRWIN_TRACK_TYPE_CDG: return TrackType.Audio;
                default: return TrackType.Data;
            }
        }

        static MediaType CdrWinIsoBusterDiscTypeToMediaType(string discType)
        {
            switch(discType)
            {
                case CDRWIN_DISK_TYPE_CD: return MediaType.CD;
                case CDRWIN_DISK_TYPE_CDRW:
                case CDRWIN_DISK_TYPE_CDMRW:
                case CDRWIN_DISK_TYPE_CDMRW2: return MediaType.CDRW;
                case CDRWIN_DISK_TYPE_DVD: return MediaType.DVDROM;
                case CDRWIN_DISK_TYPE_DVDPRW:
                case CDRWIN_DISK_TYPE_DVDPMRW:
                case CDRWIN_DISK_TYPE_DVDPMRW2: return MediaType.DVDPRW;
                case CDRWIN_DISK_TYPE_DVDPRWDL:
                case CDRWIN_DISK_TYPE_DVDPMRWDL:
                case CDRWIN_DISK_TYPE_DVDPMRWDL2: return MediaType.DVDPRWDL;
                case CDRWIN_DISK_TYPE_DVDPR:
                case CDRWIN_DISK_TYPE_DVDPVR: return MediaType.DVDPR;
                case CDRWIN_DISK_TYPE_DVDPRDL: return MediaType.DVDPRDL;
                case CDRWIN_DISK_TYPE_DVDRAM:  return MediaType.DVDRAM;
                case CDRWIN_DISK_TYPE_DVDVR:
                case CDRWIN_DISK_TYPE_DVDR: return MediaType.DVDR;
                case CDRWIN_DISK_TYPE_DVDRDL: return MediaType.DVDRDL;
                case CDRWIN_DISK_TYPE_DVDRW:
                case CDRWIN_DISK_TYPE_DVDRWDL:
                case CDRWIN_DISK_TYPE_DVDRW2: return MediaType.DVDRW;
                case CDRWIN_DISK_TYPE_HDDVD:    return MediaType.HDDVDROM;
                case CDRWIN_DISK_TYPE_HDDVDRAM: return MediaType.HDDVDRAM;
                case CDRWIN_DISK_TYPE_HDDVDR:
                case CDRWIN_DISK_TYPE_HDDVDRDL: return MediaType.HDDVDR;
                case CDRWIN_DISK_TYPE_HDDVDRW:
                case CDRWIN_DISK_TYPE_HDDVDRWDL: return MediaType.HDDVDRW;
                case CDRWIN_DISK_TYPE_BD: return MediaType.BDROM;
                case CDRWIN_DISK_TYPE_BDR:
                case CDRWIN_DISK_TYPE_BDRDL: return MediaType.BDR;
                case CDRWIN_DISK_TYPE_BDRE:
                case CDRWIN_DISK_TYPE_BDREDL: return MediaType.BDRE;
                default: return MediaType.Unknown;
            }
        }

        static (byte minute, byte second, byte frame) LbaToMsf(ulong sector) =>
            ((byte)(sector / 75 / 60), (byte)(sector / 75 % 60), (byte)(sector % 75));

        static string GetTrackMode(Track track)
        {
            switch(track.Type)
            {
                case TrackType.Audio when track.RawBytesPerSector == 2352:   return CDRWIN_TRACK_TYPE_AUDIO;
                case TrackType.Data:                                         return CDRWIN_TRACK_TYPE_MODE1;
                case TrackType.CdMode1 when track.RawBytesPerSector == 2352: return CDRWIN_TRACK_TYPE_MODE1_RAW;
                case TrackType.CdMode2Formless
                    when track.RawBytesPerSector != 2352: return CDRWIN_TRACK_TYPE_MODE2_FORMLESS;
                case TrackType.CdMode2Form1 when track.RawBytesPerSector != 2352: return CDRWIN_TRACK_TYPE_MODE2_FORM1;
                case TrackType.CdMode2Form2 when track.RawBytesPerSector != 2352: return CDRWIN_TRACK_TYPE_MODE2_FORM2;
                case TrackType.CdMode2Formless when track.RawBytesPerSector == 2352:
                case TrackType.CdMode2Form1 when track.RawBytesPerSector    == 2352:
                case TrackType.CdMode2Form2 when track.RawBytesPerSector    == 2352: return CDRWIN_TRACK_TYPE_MODE2_RAW;
                default: return CDRWIN_TRACK_TYPE_MODE1;
            }
        }

        static string MediaTypeToCdrwinType(MediaType type)
        {
            switch(type)
            {
                case MediaType.BDRXL:
                case MediaType.BDR: return CDRWIN_DISK_TYPE_BDR;
                case MediaType.BDREXL:
                case MediaType.BDRE: return CDRWIN_DISK_TYPE_BDRE;
                case MediaType.BDROM:
                case MediaType.UHDBD:
                case MediaType.CBHD:
                case MediaType.PS3BD:
                case MediaType.PS4BD:
                case MediaType.PS5BD:
                case MediaType.UDO:
                case MediaType.UDO2:
                case MediaType.UDO2_WORM: return CDRWIN_DISK_TYPE_BD;
                case MediaType.CDV:
                case MediaType.DDCD:
                case MediaType.DDCDR:
                case MediaType.DDCDRW:
                case MediaType.CDPLUS:
                case MediaType.CDR:
                case MediaType.CDROM:
                case MediaType.CDROMXA:
                case MediaType.CD:
                case MediaType.CDDA:
                case MediaType.CDEG:
                case MediaType.CDG:
                case MediaType.CDI:
                case MediaType.CDMIDI:
                case MediaType.DTSCD:
                case MediaType.JaguarCD:
                case MediaType.MEGACD:
                case MediaType.PS1CD:
                case MediaType.PS2CD:
                case MediaType.SuperCDROM2:
                case MediaType.SVCD:
                case MediaType.SVOD:
                case MediaType.SATURNCD:
                case MediaType.ThreeDO:
                case MediaType.VCD:
                case MediaType.VCDHD:
                case MediaType.MilCD:
                case MediaType.VideoNow:
                case MediaType.VideoNowColor:
                case MediaType.VideoNowXp:
                case MediaType.CVD: return CDRWIN_DISK_TYPE_CD;
                case MediaType.CDMRW:    return CDRWIN_DISK_TYPE_CDMRW;
                case MediaType.CDRW:     return CDRWIN_DISK_TYPE_CDRW;
                case MediaType.DVDPR:    return CDRWIN_DISK_TYPE_DVDPR;
                case MediaType.DVDPRDL:  return CDRWIN_DISK_TYPE_DVDPRDL;
                case MediaType.DVDPRW:   return CDRWIN_DISK_TYPE_DVDPRW;
                case MediaType.DVDPRWDL: return CDRWIN_DISK_TYPE_DVDPRWDL;
                case MediaType.DVDR:     return CDRWIN_DISK_TYPE_DVDR;
                case MediaType.DVDRAM:   return CDRWIN_DISK_TYPE_DVDRAM;
                case MediaType.DVDRDL:   return CDRWIN_DISK_TYPE_DVDRDL;
                case MediaType.DVDDownload:
                case MediaType.DVDROM:
                case MediaType.UMD:
                case MediaType.PS2DVD:
                case MediaType.PS3DVD: return CDRWIN_DISK_TYPE_DVD;
                case MediaType.DVDRW:     return CDRWIN_DISK_TYPE_DVDRW;
                case MediaType.DVDRWDL:   return CDRWIN_DISK_TYPE_DVDRWDL;
                case MediaType.HDDVDR:    return CDRWIN_DISK_TYPE_HDDVDR;
                case MediaType.HDDVDRAM:  return CDRWIN_DISK_TYPE_HDDVDRAM;
                case MediaType.HDDVDRDL:  return CDRWIN_DISK_TYPE_HDDVDRDL;
                case MediaType.HDDVDROM:  return CDRWIN_DISK_TYPE_HDDVD;
                case MediaType.HDDVDRW:   return CDRWIN_DISK_TYPE_HDDVDRW;
                case MediaType.HDDVDRWDL: return CDRWIN_DISK_TYPE_HDDVDRWDL;
                default:                  return "";
            }
        }
    }
}