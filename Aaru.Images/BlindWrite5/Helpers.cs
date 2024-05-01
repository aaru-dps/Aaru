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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes;
using Aaru.Decoders.SCSI.MMC;

namespace Aaru.Images;

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
        return profile switch
               {
                   ProfileNumber.BDRE                                  => MediaType.BDRE,
                   ProfileNumber.BDROM                                 => MediaType.BDROM,
                   ProfileNumber.BDRRdm or ProfileNumber.BDRSeq        => MediaType.BDR,
                   ProfileNumber.CDR or ProfileNumber.HDBURNR          => MediaType.CDR,
                   ProfileNumber.CDROM or ProfileNumber.HDBURNROM      => MediaType.CDROM,
                   ProfileNumber.CDRW or ProfileNumber.HDBURNRW        => MediaType.CDRW,
                   ProfileNumber.DDCDR                                 => MediaType.DDCDR,
                   ProfileNumber.DDCDROM                               => MediaType.DDCD,
                   ProfileNumber.DDCDRW                                => MediaType.DDCDRW,
                   ProfileNumber.DVDDownload                           => MediaType.DVDDownload,
                   ProfileNumber.DVDRAM                                => MediaType.DVDRAM,
                   ProfileNumber.DVDRDLJump or ProfileNumber.DVDRDLSeq => MediaType.DVDRDL,
                   ProfileNumber.DVDRDLPlus                            => MediaType.DVDPRDL,
                   ProfileNumber.DVDROM                                => MediaType.DVDROM,
                   ProfileNumber.DVDRPlus                              => MediaType.DVDPR,
                   ProfileNumber.DVDRSeq                               => MediaType.DVDR,
                   ProfileNumber.DVDRWDL                               => MediaType.DVDRWDL,
                   ProfileNumber.DVDRWDLPlus                           => MediaType.DVDPRWDL,
                   ProfileNumber.DVDRWPlus                             => MediaType.DVDPRW,
                   ProfileNumber.DVDRWRes or ProfileNumber.DVDRWSeq    => MediaType.DVDRW,
                   ProfileNumber.HDDVDR                                => MediaType.HDDVDR,
                   ProfileNumber.HDDVDRAM                              => MediaType.HDDVDRAM,
                   ProfileNumber.HDDVDRDL                              => MediaType.HDDVDRDL,
                   ProfileNumber.HDDVDROM                              => MediaType.HDDVDROM,
                   ProfileNumber.HDDVDRW                               => MediaType.HDDVDRW,
                   ProfileNumber.HDDVDRWDL                             => MediaType.HDDVDRWDL,
                   ProfileNumber.ASMO or ProfileNumber.MOErasable      => MediaType.UnknownMO,
                   ProfileNumber.NonRemovable                          => MediaType.GENERIC_HDD,
                   _                                                   => MediaType.CD
               };
    }
}