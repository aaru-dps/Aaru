// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for Alcohol 120% disc images.
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

using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;

namespace Aaru.DiscImages
{
    public sealed partial class Alcohol120
    {
        static ushort TrackModeToCookedBytesPerSector(TrackMode trackMode)
        {
            switch(trackMode)
            {
                case TrackMode.Mode1:
                case TrackMode.Mode1Alt:
                case TrackMode.Mode2F1:
                case TrackMode.Mode2F1Alt: return 2048;
                case TrackMode.Mode2F2:
                case TrackMode.Mode2F2Alt: return 2324;
                case TrackMode.Mode2: return 2336;
                case TrackMode.Audio:
                case TrackMode.AudioAlt: return 2352;
                case TrackMode.DVD: return 2048;
                default:            return 0;
            }
        }

        static TrackType TrackModeToTrackType(TrackMode trackType)
        {
            switch(trackType)
            {
                case TrackMode.Mode1:
                case TrackMode.Mode1Alt: return TrackType.CdMode1;
                case TrackMode.Mode2F1:
                case TrackMode.Mode2F1Alt: return TrackType.CdMode2Form1;
                case TrackMode.Mode2F2:
                case TrackMode.Mode2F2Alt: return TrackType.CdMode2Form2;
                case TrackMode.Mode2: return TrackType.CdMode2Formless;
                case TrackMode.Audio:
                case TrackMode.AudioAlt: return TrackType.Audio;
                default: return TrackType.Data;
            }
        }

        static MediaType MediumTypeToMediaType(MediumType discType)
        {
            switch(discType)
            {
                case MediumType.CD:   return MediaType.CD;
                case MediumType.CDR:  return MediaType.CDR;
                case MediumType.CDRW: return MediaType.CDRW;
                case MediumType.DVD:  return MediaType.DVDROM;
                case MediumType.DVDR: return MediaType.DVDR;
                default:              return MediaType.Unknown;
            }
        }

        static MediumType MediaTypeToMediumType(MediaType type)
        {
            switch(type)
            {
                case MediaType.CD:
                case MediaType.CDDA:
                case MediaType.CDEG:
                case MediaType.CDG:
                case MediaType.CDI:
                case MediaType.CDMIDI:
                case MediaType.CDPLUS:
                case MediaType.CDROM:
                case MediaType.CDROMXA:
                case MediaType.CDV:
                case MediaType.DTSCD:
                case MediaType.JaguarCD:
                case MediaType.MEGACD:
                case MediaType.PS1CD:
                case MediaType.PS2CD:
                case MediaType.SuperCDROM2:
                case MediaType.SVCD:
                case MediaType.SATURNCD:
                case MediaType.ThreeDO:
                case MediaType.VCD:
                case MediaType.VCDHD:
                case MediaType.NeoGeoCD:
                case MediaType.PCFX:
                case MediaType.CDTV:
                case MediaType.CD32:
                case MediaType.Nuon:
                case MediaType.Playdia:
                case MediaType.Pippin:
                case MediaType.FMTOWNS:
                case MediaType.MilCD:
                case MediaType.VideoNow:
                case MediaType.VideoNowColor:
                case MediaType.VideoNowXp:
                case MediaType.CVD: return MediumType.CD;
                case MediaType.CDR: return MediumType.CDR;
                case MediaType.CDRW:
                case MediaType.CDMRW: return MediumType.CDRW;
                case MediaType.DVDR:
                case MediaType.DVDRW:
                case MediaType.DVDPR:
                case MediaType.DVDRDL:
                case MediaType.DVDRWDL:
                case MediaType.DVDPRDL:
                case MediaType.DVDPRWDL: return MediumType.DVDR;
                default: return MediumType.DVD;
            }
        }

        static TrackMode TrackTypeToTrackMode(TrackType type)
        {
            switch(type)
            {
                case TrackType.Audio:           return TrackMode.Audio;
                case TrackType.CdMode1:         return TrackMode.Mode1;
                case TrackType.CdMode2Formless: return TrackMode.Mode2;
                case TrackType.CdMode2Form1:    return TrackMode.Mode2F1;
                case TrackType.CdMode2Form2:    return TrackMode.Mode2F2;
                default:                        return TrackMode.DVD;
            }
        }

        static (byte minute, byte second, byte frame) LbaToMsf(ulong sector) =>
            ((byte)((sector + 150) / 75 / 60), (byte)((sector + 150) / 75 % 60), (byte)((sector + 150) % 75));
    }
}