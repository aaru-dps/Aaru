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
//     Contains helpers for BlindWrite 5 disc images.
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
using Aaru.Decoders.SCSI.MMC;

namespace Aaru.DiscImages;

public sealed partial class BlindWrite5
{
    static CommonTypes.Enums.TrackType BlindWriteTrackTypeToTrackType(TrackType trackType) => trackType switch
    {
        TrackType.Mode1   => CommonTypes.Enums.TrackType.CdMode1,
        TrackType.Mode2F1 => CommonTypes.Enums.TrackType.CdMode2Form1,
        TrackType.Mode2F2 => CommonTypes.Enums.TrackType.CdMode2Form2,
        TrackType.Mode2   => CommonTypes.Enums.TrackType.CdMode2Formless,
        TrackType.Audio   => CommonTypes.Enums.TrackType.Audio,
        _                 => CommonTypes.Enums.TrackType.Data
    };

    static MediaType BlindWriteProfileToMediaType(ProfileNumber profile)
    {
        switch(profile)
        {
            case ProfileNumber.BDRE:  return MediaType.BDRE;
            case ProfileNumber.BDROM: return MediaType.BDROM;
            case ProfileNumber.BDRRdm:
            case ProfileNumber.BDRSeq: return MediaType.BDR;
            case ProfileNumber.CDR:
            case ProfileNumber.HDBURNR: return MediaType.CDR;
            case ProfileNumber.CDROM:
            case ProfileNumber.HDBURNROM: return MediaType.CDROM;
            case ProfileNumber.CDRW:
            case ProfileNumber.HDBURNRW: return MediaType.CDRW;
            case ProfileNumber.DDCDR:       return MediaType.DDCDR;
            case ProfileNumber.DDCDROM:     return MediaType.DDCD;
            case ProfileNumber.DDCDRW:      return MediaType.DDCDRW;
            case ProfileNumber.DVDDownload: return MediaType.DVDDownload;
            case ProfileNumber.DVDRAM:      return MediaType.DVDRAM;
            case ProfileNumber.DVDRDLJump:
            case ProfileNumber.DVDRDLSeq: return MediaType.DVDRDL;
            case ProfileNumber.DVDRDLPlus:  return MediaType.DVDPRDL;
            case ProfileNumber.DVDROM:      return MediaType.DVDROM;
            case ProfileNumber.DVDRPlus:    return MediaType.DVDPR;
            case ProfileNumber.DVDRSeq:     return MediaType.DVDR;
            case ProfileNumber.DVDRWDL:     return MediaType.DVDRWDL;
            case ProfileNumber.DVDRWDLPlus: return MediaType.DVDPRWDL;
            case ProfileNumber.DVDRWPlus:   return MediaType.DVDPRW;
            case ProfileNumber.DVDRWRes:
            case ProfileNumber.DVDRWSeq: return MediaType.DVDRW;
            case ProfileNumber.HDDVDR:    return MediaType.HDDVDR;
            case ProfileNumber.HDDVDRAM:  return MediaType.HDDVDRAM;
            case ProfileNumber.HDDVDRDL:  return MediaType.HDDVDRDL;
            case ProfileNumber.HDDVDROM:  return MediaType.HDDVDROM;
            case ProfileNumber.HDDVDRW:   return MediaType.HDDVDRW;
            case ProfileNumber.HDDVDRWDL: return MediaType.HDDVDRWDL;
            case ProfileNumber.ASMO:
            case ProfileNumber.MOErasable: return MediaType.UnknownMO;
            case ProfileNumber.NonRemovable: return MediaType.GENERIC_HDD;
            default:                         return MediaType.CD;
        }
    }
}