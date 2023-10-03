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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Enums;

namespace Aaru.DiscImages;

public sealed partial class Nero
{
    static CommonTypes.MediaType NeroMediaTypeToMediaType(NeroMediaTypes type)
    {
        switch(type)
        {
            case NeroMediaTypes.NeroMtypDdcd:
                return CommonTypes.MediaType.DDCD;
            case NeroMediaTypes.NeroMtypDvdM:
            case NeroMediaTypes.NeroMtypDvdMR:
                return CommonTypes.MediaType.DVDR;
            case NeroMediaTypes.NeroMtypDvdP:
            case NeroMediaTypes.NeroMtypDvdPR:
                return CommonTypes.MediaType.DVDPR;
            case NeroMediaTypes.NeroMtypDvdRam:
                return CommonTypes.MediaType.DVDRAM;
            case NeroMediaTypes.NeroMtypMl:
            case NeroMediaTypes.NeroMtypMrw:
            case NeroMediaTypes.NeroMtypCdrw:
                return CommonTypes.MediaType.CDRW;
            case NeroMediaTypes.NeroMtypCdr:
                return CommonTypes.MediaType.CDR;
            case NeroMediaTypes.NeroMtypDvdRom:
            case NeroMediaTypes.NeroMtypDvdAny:
            case NeroMediaTypes.NeroMtypDvdAnyR9:
            case NeroMediaTypes.NeroMtypDvdAnyOld:
                return CommonTypes.MediaType.DVDROM;
            case NeroMediaTypes.NeroMtypCdrom:
                return CommonTypes.MediaType.CDROM;
            case NeroMediaTypes.NeroMtypDvdMRw:
                return CommonTypes.MediaType.DVDRW;
            case NeroMediaTypes.NeroMtypDvdPRw:
                return CommonTypes.MediaType.DVDPRW;
            case NeroMediaTypes.NeroMtypDvdPR9:
                return CommonTypes.MediaType.DVDPRDL;
            case NeroMediaTypes.NeroMtypDvdMR9:
                return CommonTypes.MediaType.DVDRDL;
            case NeroMediaTypes.NeroMtypBd:
            case NeroMediaTypes.NeroMtypBdAny:
            case NeroMediaTypes.NeroMtypBdRom:
                return CommonTypes.MediaType.BDROM;
            case NeroMediaTypes.NeroMtypBdR:
                return CommonTypes.MediaType.BDR;
            case NeroMediaTypes.NeroMtypBdRe:
                return CommonTypes.MediaType.BDRE;
            case NeroMediaTypes.NeroMtypHdDvd:
            case NeroMediaTypes.NeroMtypHdDvdAny:
            case NeroMediaTypes.NeroMtypHdDvdRom:
                return CommonTypes.MediaType.HDDVDROM;
            case NeroMediaTypes.NeroMtypHdDvdR:
                return CommonTypes.MediaType.HDDVDR;
            case NeroMediaTypes.NeroMtypHdDvdRw:
                return CommonTypes.MediaType.HDDVDRW;
            default:
                return CommonTypes.MediaType.CD;
        }
    }

    static TrackType NeroTrackModeToTrackType(DaoMode mode)
    {
        switch(mode)
        {
            case DaoMode.Data:
            case DaoMode.DataRaw:
            case DaoMode.DataRawSub:
                return TrackType.CdMode1;
            case DaoMode.DataM2F1:
                return TrackType.CdMode2Form1;
            case DaoMode.DataM2F2:
                return TrackType.CdMode2Form2;
            case DaoMode.DataM2RawSub:
            case DaoMode.DataM2Raw:
                return TrackType.CdMode2Formless;
            case DaoMode.Audio:
            case DaoMode.AudioAlt:
            case DaoMode.AudioSub:
                return TrackType.Audio;
            default:
                return TrackType.Data;
        }
    }

    static ushort NeroTrackModeToBytesPerSector(DaoMode mode)
    {
        switch(mode)
        {
            case DaoMode.Data:
            case DaoMode.DataM2F1:
                return 2048;
            case DaoMode.DataM2F2:
                return 2336;
            case DaoMode.DataRaw:
            case DaoMode.DataM2Raw:
            case DaoMode.AudioAlt:
            case DaoMode.Audio:
                return 2352;
            case DaoMode.DataM2RawSub:
            case DaoMode.DataRawSub:
            case DaoMode.AudioSub:
                return 2448;
            default:
                return 2352;
        }
    }
}