// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;

namespace DiscImageChef.DiscImages
{
    public partial class Alcohol120
    {
        static ushort AlcoholTrackModeToBytesPerSector(AlcoholTrackMode trackMode)
        {
            switch(trackMode)
            {
                case AlcoholTrackMode.Audio:
                case AlcoholTrackMode.Mode1:
                case AlcoholTrackMode.Mode2:
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F2:
                case AlcoholTrackMode.Mode2F2Alt:
                case AlcoholTrackMode.Mode2F1Alt: return 2352;
                case AlcoholTrackMode.DVD: return 2048;
                default:                   return 0;
            }
        }

        static ushort AlcoholTrackModeToCookedBytesPerSector(AlcoholTrackMode trackMode)
        {
            switch(trackMode)
            {
                case AlcoholTrackMode.Mode1:
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt: return 2048;
                case AlcoholTrackMode.Mode2F2:
                case AlcoholTrackMode.Mode2F2Alt: return 2324;
                case AlcoholTrackMode.Mode2: return 2336;
                case AlcoholTrackMode.Audio: return 2352;
                case AlcoholTrackMode.DVD:   return 2048;
                default:                     return 0;
            }
        }

        static TrackType AlcoholTrackTypeToTrackType(AlcoholTrackMode trackType)
        {
            switch(trackType)
            {
                case AlcoholTrackMode.Mode1: return TrackType.CdMode1;
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt: return TrackType.CdMode2Form1;
                case AlcoholTrackMode.Mode2F2:
                case AlcoholTrackMode.Mode2F2Alt: return TrackType.CdMode2Form2;
                case AlcoholTrackMode.Mode2: return TrackType.CdMode2Formless;
                case AlcoholTrackMode.Audio: return TrackType.Audio;
                default:                     return TrackType.Data;
            }
        }

        static MediaType AlcoholMediumTypeToMediaType(AlcoholMediumType discType)
        {
            switch(discType)
            {
                case AlcoholMediumType.CD:   return MediaType.CD;
                case AlcoholMediumType.CDR:  return MediaType.CDR;
                case AlcoholMediumType.CDRW: return MediaType.CDRW;
                case AlcoholMediumType.DVD:  return MediaType.DVDROM;
                case AlcoholMediumType.DVDR: return MediaType.DVDR;
                default:                     return MediaType.Unknown;
            }
        }

        static AlcoholMediumType MediaTypeToAlcohol(MediaType type)
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
                case MediaType.FMTOWNS: return AlcoholMediumType.CD;
                case MediaType.CDR: return AlcoholMediumType.CDR;
                case MediaType.CDRW:
                case MediaType.CDMRW: return AlcoholMediumType.CDRW;
                case MediaType.DVDR:
                case MediaType.DVDRW:
                case MediaType.DVDPR:
                case MediaType.DVDRDL:
                case MediaType.DVDRWDL:
                case MediaType.DVDPRDL:
                case MediaType.DVDPRWDL: return AlcoholMediumType.DVDR;
                default: return AlcoholMediumType.DVD;
            }
        }

        static AlcoholTrackMode TrackTypeToAlcohol(TrackType type)
        {
            switch(type)
            {
                case TrackType.Audio:           return AlcoholTrackMode.Audio;
                case TrackType.CdMode1:         return AlcoholTrackMode.Mode1;
                case TrackType.CdMode2Formless: return AlcoholTrackMode.Mode2;
                case TrackType.CdMode2Form1:    return AlcoholTrackMode.Mode2F1;
                case TrackType.CdMode2Form2:    return AlcoholTrackMode.Mode2F2;
                default:                        return AlcoholTrackMode.DVD;
            }
        }

        static (byte minute, byte second, byte frame) LbaToMsf(ulong sector) =>
            ((byte)((sector + 150) / 75 / 60), (byte)((sector + 150) / 75 % 60), (byte)((sector + 150) % 75));
    }
}