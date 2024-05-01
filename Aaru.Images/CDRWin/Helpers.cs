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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

public sealed partial class CdrWin
{
    static int CdrWinMsfToLba(string msf)
    {
        string[] msfElements = msf.Split(':');
        var      minute      = int.Parse(msfElements[0]);
        var      second      = int.Parse(msfElements[1]);
        var      frame       = int.Parse(msfElements[2]);

        int sectors = minute * 60 * 75 + second * 75 + frame;

        return sectors;
    }

    static ushort CdrWinTrackTypeToBytesPerSector(string trackType)
    {
        return trackType switch
               {
                   CDRWIN_TRACK_TYPE_MODE1 or CDRWIN_TRACK_TYPE_MODE2_FORM1  => 2048,
                   CDRWIN_TRACK_TYPE_MODE2_FORM2                             => 2324,
                   CDRWIN_TRACK_TYPE_MODE2_FORMLESS or CDRWIN_TRACK_TYPE_CDI => 2336,
                   CDRWIN_TRACK_TYPE_AUDIO
                    or CDRWIN_TRACK_TYPE_MODE1_RAW
                    or CDRWIN_TRACK_TYPE_MODE2_RAW
                    or CDRWIN_TRACK_TYPE_CDI_RAW => 2352,
                   CDRWIN_TRACK_TYPE_CDG => 2448,
                   _                     => 0
               };
    }

    static ushort CdrWinTrackTypeToCookedBytesPerSector(string trackType)
    {
        return trackType switch
               {
                   CDRWIN_TRACK_TYPE_MODE1 or CDRWIN_TRACK_TYPE_MODE2_FORM1 or CDRWIN_TRACK_TYPE_MODE1_RAW => 2048,
                   CDRWIN_TRACK_TYPE_MODE2_FORM2                                                           => 2324,
                   CDRWIN_TRACK_TYPE_MODE2_FORMLESS
                    or CDRWIN_TRACK_TYPE_CDI
                    or CDRWIN_TRACK_TYPE_MODE2_RAW
                    or CDRWIN_TRACK_TYPE_CDI_RAW => 2336,
                   CDRWIN_TRACK_TYPE_CDG or CDRWIN_TRACK_TYPE_AUDIO => 2352,
                   _                                                => 0
               };
    }

    static TrackType CdrWinTrackTypeToTrackType(string trackType)
    {
        return trackType switch
               {
                   CDRWIN_TRACK_TYPE_MODE1 or CDRWIN_TRACK_TYPE_MODE1_RAW => TrackType.CdMode1,
                   CDRWIN_TRACK_TYPE_MODE2_FORM1                          => TrackType.CdMode2Form1,
                   CDRWIN_TRACK_TYPE_MODE2_FORM2                          => TrackType.CdMode2Form2,
                   CDRWIN_TRACK_TYPE_CDI_RAW
                    or CDRWIN_TRACK_TYPE_CDI
                    or CDRWIN_TRACK_TYPE_MODE2_RAW
                    or CDRWIN_TRACK_TYPE_MODE2_FORMLESS => TrackType.CdMode2Formless,
                   CDRWIN_TRACK_TYPE_AUDIO or CDRWIN_TRACK_TYPE_CDG => TrackType.Audio,
                   _                                                => TrackType.Data
               };
    }

    static MediaType CdrWinIsoBusterDiscTypeToMediaType(string discType)
    {
        return discType switch
               {
                   CDRWIN_DISK_TYPE_CD                                                              => MediaType.CD,
                   CDRWIN_DISK_TYPE_CDRW or CDRWIN_DISK_TYPE_CDMRW or CDRWIN_DISK_TYPE_CDMRW2       => MediaType.CDRW,
                   CDRWIN_DISK_TYPE_DVD                                                             => MediaType.DVDROM,
                   CDRWIN_DISK_TYPE_DVDPRW or CDRWIN_DISK_TYPE_DVDPMRW or CDRWIN_DISK_TYPE_DVDPMRW2 => MediaType.DVDPRW,
                   CDRWIN_DISK_TYPE_DVDPRWDL or CDRWIN_DISK_TYPE_DVDPMRWDL or CDRWIN_DISK_TYPE_DVDPMRWDL2 => MediaType
                      .DVDPRWDL,
                   CDRWIN_DISK_TYPE_DVDPR or CDRWIN_DISK_TYPE_DVDPVR                             => MediaType.DVDPR,
                   CDRWIN_DISK_TYPE_DVDPRDL                                                      => MediaType.DVDPRDL,
                   CDRWIN_DISK_TYPE_DVDRAM                                                       => MediaType.DVDRAM,
                   CDRWIN_DISK_TYPE_DVDVR or CDRWIN_DISK_TYPE_DVDR                               => MediaType.DVDR,
                   CDRWIN_DISK_TYPE_DVDRDL                                                       => MediaType.DVDRDL,
                   CDRWIN_DISK_TYPE_DVDRW or CDRWIN_DISK_TYPE_DVDRWDL or CDRWIN_DISK_TYPE_DVDRW2 => MediaType.DVDRW,
                   CDRWIN_DISK_TYPE_HDDVD                                                        => MediaType.HDDVDROM,
                   CDRWIN_DISK_TYPE_HDDVDRAM                                                     => MediaType.HDDVDRAM,
                   CDRWIN_DISK_TYPE_HDDVDR or CDRWIN_DISK_TYPE_HDDVDRDL                          => MediaType.HDDVDR,
                   CDRWIN_DISK_TYPE_HDDVDRW or CDRWIN_DISK_TYPE_HDDVDRWDL                        => MediaType.HDDVDRW,
                   CDRWIN_DISK_TYPE_BD                                                           => MediaType.BDROM,
                   CDRWIN_DISK_TYPE_BDR or CDRWIN_DISK_TYPE_BDRDL                                => MediaType.BDR,
                   CDRWIN_DISK_TYPE_BDRE or CDRWIN_DISK_TYPE_BDREDL                              => MediaType.BDRE,
                   _                                                                             => MediaType.Unknown
               };
    }

    static (byte minute, byte second, byte frame) LbaToMsf(ulong sector) =>
        ((byte)(sector / 75 / 60), (byte)(sector / 75 % 60), (byte)(sector % 75));

    static string GetTrackMode(Track track)
    {
        switch(track.Type)
        {
            case TrackType.Audio when track.RawBytesPerSector == 2352:
                return CDRWIN_TRACK_TYPE_AUDIO;
            case TrackType.Data:
                return CDRWIN_TRACK_TYPE_MODE1;
            case TrackType.CdMode1 when track.RawBytesPerSector == 2352:
                return CDRWIN_TRACK_TYPE_MODE1_RAW;
            case TrackType.CdMode2Formless when track.RawBytesPerSector != 2352:
                return CDRWIN_TRACK_TYPE_MODE2_FORMLESS;
            case TrackType.CdMode2Form1 when track.RawBytesPerSector != 2352:
                return CDRWIN_TRACK_TYPE_MODE2_FORM1;
            case TrackType.CdMode2Form2 when track.RawBytesPerSector != 2352:
                return CDRWIN_TRACK_TYPE_MODE2_FORM2;
            case TrackType.CdMode2Formless when track.RawBytesPerSector == 2352:
            case TrackType.CdMode2Form1 when track.RawBytesPerSector    == 2352:
            case TrackType.CdMode2Form2 when track.RawBytesPerSector    == 2352:
                return CDRWIN_TRACK_TYPE_MODE2_RAW;
            default:
                return CDRWIN_TRACK_TYPE_MODE1;
        }
    }

    static string MediaTypeToCdrwinType(MediaType type)
    {
        return type switch
               {
                   MediaType.BDRXL or MediaType.BDR   => CDRWIN_DISK_TYPE_BDR,
                   MediaType.BDREXL or MediaType.BDRE => CDRWIN_DISK_TYPE_BDRE,
                   MediaType.BDROM
                    or MediaType.UHDBD
                    or MediaType.CBHD
                    or MediaType.PS3BD
                    or MediaType.PS4BD
                    or MediaType.PS5BD
                    or MediaType.UDO
                    or MediaType.UDO2
                    or MediaType.UDO2_WORM => CDRWIN_DISK_TYPE_BD,
                   MediaType.CDV
                    or MediaType.DDCD
                    or MediaType.DDCDR
                    or MediaType.DDCDRW
                    or MediaType.CDPLUS
                    or MediaType.CDR
                    or MediaType.CDROM
                    or MediaType.CDROMXA
                    or MediaType.CD
                    or MediaType.CDDA
                    or MediaType.CDEG
                    or MediaType.CDG
                    or MediaType.CDI
                    or MediaType.CDMIDI
                    or MediaType.DTSCD
                    or MediaType.JaguarCD
                    or MediaType.MEGACD
                    or MediaType.PS1CD
                    or MediaType.PS2CD
                    or MediaType.SuperCDROM2
                    or MediaType.SVCD
                    or MediaType.SVOD
                    or MediaType.SATURNCD
                    or MediaType.ThreeDO
                    or MediaType.VCD
                    or MediaType.VCDHD
                    or MediaType.MilCD
                    or MediaType.VideoNow
                    or MediaType.VideoNowColor
                    or MediaType.VideoNowXp
                    or MediaType.CVD => CDRWIN_DISK_TYPE_CD,
                   MediaType.CDMRW    => CDRWIN_DISK_TYPE_CDMRW,
                   MediaType.CDRW     => CDRWIN_DISK_TYPE_CDRW,
                   MediaType.DVDPR    => CDRWIN_DISK_TYPE_DVDPR,
                   MediaType.DVDPRDL  => CDRWIN_DISK_TYPE_DVDPRDL,
                   MediaType.DVDPRW   => CDRWIN_DISK_TYPE_DVDPRW,
                   MediaType.DVDPRWDL => CDRWIN_DISK_TYPE_DVDPRWDL,
                   MediaType.DVDR     => CDRWIN_DISK_TYPE_DVDR,
                   MediaType.DVDRAM   => CDRWIN_DISK_TYPE_DVDRAM,
                   MediaType.DVDRDL   => CDRWIN_DISK_TYPE_DVDRDL,
                   MediaType.DVDDownload or MediaType.DVDROM or MediaType.UMD or MediaType.PS2DVD or MediaType.PS3DVD =>
                       CDRWIN_DISK_TYPE_DVD,
                   MediaType.DVDRW     => CDRWIN_DISK_TYPE_DVDRW,
                   MediaType.DVDRWDL   => CDRWIN_DISK_TYPE_DVDRWDL,
                   MediaType.HDDVDR    => CDRWIN_DISK_TYPE_HDDVDR,
                   MediaType.HDDVDRAM  => CDRWIN_DISK_TYPE_HDDVDRAM,
                   MediaType.HDDVDRDL  => CDRWIN_DISK_TYPE_HDDVDRDL,
                   MediaType.HDDVDROM  => CDRWIN_DISK_TYPE_HDDVD,
                   MediaType.HDDVDRW   => CDRWIN_DISK_TYPE_HDDVDRW,
                   MediaType.HDDVDRWDL => CDRWIN_DISK_TYPE_HDDVDRWDL,
                   _                   => ""
               };
    }
}