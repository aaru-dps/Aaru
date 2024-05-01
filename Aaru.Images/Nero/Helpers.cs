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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Enums;

namespace Aaru.Images;

public sealed partial class Nero
{
    static CommonTypes.MediaType NeroMediaTypeToMediaType(NeroMediaTypes type)
    {
        return type switch
               {
                   NeroMediaTypes.NeroMtypDdcd                                 => CommonTypes.MediaType.DDCD,
                   NeroMediaTypes.NeroMtypDvdM or NeroMediaTypes.NeroMtypDvdMR => CommonTypes.MediaType.DVDR,
                   NeroMediaTypes.NeroMtypDvdP or NeroMediaTypes.NeroMtypDvdPR => CommonTypes.MediaType.DVDPR,
                   NeroMediaTypes.NeroMtypDvdRam                               => CommonTypes.MediaType.DVDRAM,
                   NeroMediaTypes.NeroMtypMl or NeroMediaTypes.NeroMtypMrw or NeroMediaTypes.NeroMtypCdrw => CommonTypes
                      .MediaType.CDRW,
                   NeroMediaTypes.NeroMtypCdr => CommonTypes.MediaType.CDR,
                   NeroMediaTypes.NeroMtypDvdRom
                    or NeroMediaTypes.NeroMtypDvdAny
                    or NeroMediaTypes.NeroMtypDvdAnyR9
                    or NeroMediaTypes.NeroMtypDvdAnyOld => CommonTypes.MediaType.DVDROM,
                   NeroMediaTypes.NeroMtypCdrom  => CommonTypes.MediaType.CDROM,
                   NeroMediaTypes.NeroMtypDvdMRw => CommonTypes.MediaType.DVDRW,
                   NeroMediaTypes.NeroMtypDvdPRw => CommonTypes.MediaType.DVDPRW,
                   NeroMediaTypes.NeroMtypDvdPR9 => CommonTypes.MediaType.DVDPRDL,
                   NeroMediaTypes.NeroMtypDvdMR9 => CommonTypes.MediaType.DVDRDL,
                   NeroMediaTypes.NeroMtypBd or NeroMediaTypes.NeroMtypBdAny or NeroMediaTypes.NeroMtypBdRom =>
                       CommonTypes.MediaType.BDROM,
                   NeroMediaTypes.NeroMtypBdR  => CommonTypes.MediaType.BDR,
                   NeroMediaTypes.NeroMtypBdRe => CommonTypes.MediaType.BDRE,
                   NeroMediaTypes.NeroMtypHdDvd or NeroMediaTypes.NeroMtypHdDvdAny or NeroMediaTypes.NeroMtypHdDvdRom =>
                       CommonTypes.MediaType.HDDVDROM,
                   NeroMediaTypes.NeroMtypHdDvdR  => CommonTypes.MediaType.HDDVDR,
                   NeroMediaTypes.NeroMtypHdDvdRw => CommonTypes.MediaType.HDDVDRW,
                   _                              => CommonTypes.MediaType.CD
               };
    }

    static TrackType NeroTrackModeToTrackType(DaoMode mode)
    {
        return mode switch
               {
                   DaoMode.Data or DaoMode.DataRaw or DaoMode.DataRawSub => TrackType.CdMode1,
                   DaoMode.DataM2F1                                      => TrackType.CdMode2Form1,
                   DaoMode.DataM2F2                                      => TrackType.CdMode2Form2,
                   DaoMode.DataM2RawSub or DaoMode.DataM2Raw             => TrackType.CdMode2Formless,
                   DaoMode.Audio or DaoMode.AudioAlt or DaoMode.AudioSub => TrackType.Audio,
                   _                                                     => TrackType.Data
               };
    }

    static ushort NeroTrackModeToBytesPerSector(DaoMode mode)
    {
        return mode switch
               {
                   DaoMode.Data or DaoMode.DataM2F1                                          => 2048,
                   DaoMode.DataM2F2                                                          => 2336,
                   DaoMode.DataRaw or DaoMode.DataM2Raw or DaoMode.AudioAlt or DaoMode.Audio => 2352,
                   DaoMode.DataM2RawSub or DaoMode.DataRawSub or DaoMode.AudioSub            => 2448,
                   _                                                                         => 2352
               };
    }
}