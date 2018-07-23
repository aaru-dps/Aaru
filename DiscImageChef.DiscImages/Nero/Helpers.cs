// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for Nero Burning ROM disc images.
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
    public partial class Nero
    {
        static MediaType NeroMediaTypeToMediaType(NeroMediaTypes type)
        {
            switch(type)
            {
                case NeroMediaTypes.NeroMtypDdcd: return MediaType.DDCD;
                case NeroMediaTypes.NeroMtypDvdM:
                case NeroMediaTypes.NeroMtypDvdMR: return MediaType.DVDR;
                case NeroMediaTypes.NeroMtypDvdP:
                case NeroMediaTypes.NeroMtypDvdPR: return MediaType.DVDPR;
                case NeroMediaTypes.NeroMtypDvdRam: return MediaType.DVDRAM;
                case NeroMediaTypes.NeroMtypMl:
                case NeroMediaTypes.NeroMtypMrw:
                case NeroMediaTypes.NeroMtypCdrw: return MediaType.CDRW;
                case NeroMediaTypes.NeroMtypCdr: return MediaType.CDR;
                case NeroMediaTypes.NeroMtypDvdRom:
                case NeroMediaTypes.NeroMtypDvdAny:
                case NeroMediaTypes.NeroMtypDvdAnyR9:
                case NeroMediaTypes.NeroMtypDvdAnyOld: return MediaType.DVDROM;
                case NeroMediaTypes.NeroMtypCdrom:  return MediaType.CDROM;
                case NeroMediaTypes.NeroMtypDvdMRw: return MediaType.DVDRW;
                case NeroMediaTypes.NeroMtypDvdPRw: return MediaType.DVDPRW;
                case NeroMediaTypes.NeroMtypDvdPR9: return MediaType.DVDPRDL;
                case NeroMediaTypes.NeroMtypDvdMR9: return MediaType.DVDRDL;
                case NeroMediaTypes.NeroMtypBd:
                case NeroMediaTypes.NeroMtypBdAny:
                case NeroMediaTypes.NeroMtypBdRom: return MediaType.BDROM;
                case NeroMediaTypes.NeroMtypBdR:  return MediaType.BDR;
                case NeroMediaTypes.NeroMtypBdRe: return MediaType.BDRE;
                case NeroMediaTypes.NeroMtypHdDvd:
                case NeroMediaTypes.NeroMtypHdDvdAny:
                case NeroMediaTypes.NeroMtypHdDvdRom: return MediaType.HDDVDROM;
                case NeroMediaTypes.NeroMtypHdDvdR:  return MediaType.HDDVDR;
                case NeroMediaTypes.NeroMtypHdDvdRw: return MediaType.HDDVDRW;
                default:                             return MediaType.CD;
            }
        }

        static TrackType NeroTrackModeToTrackType(DaoMode mode)
        {
            switch(mode)
            {
                case DaoMode.Data:
                case DaoMode.DataRaw:
                case DaoMode.DataRawSub: return TrackType.CdMode1;
                case DaoMode.DataM2F1: return TrackType.CdMode2Form1;
                case DaoMode.DataM2F2: return TrackType.CdMode2Form2;
                case DaoMode.DataM2RawSub:
                case DaoMode.DataM2Raw: return TrackType.CdMode2Formless;
                case DaoMode.Audio:
                case DaoMode.AudioSub: return TrackType.Audio;
                default: return TrackType.Data;
            }
        }

        static ushort NeroTrackModeToBytesPerSector(DaoMode mode)
        {
            switch(mode)
            {
                case DaoMode.Data:
                case DaoMode.DataM2F1: return 2048;
                case DaoMode.DataM2F2: return 2336;
                case DaoMode.DataRaw:
                case DaoMode.DataM2Raw:
                case DaoMode.Audio: return 2352;
                case DaoMode.DataM2RawSub:
                case DaoMode.DataRawSub:
                case DaoMode.AudioSub: return 2448;
                default: return 2352;
            }
        }
    }
}