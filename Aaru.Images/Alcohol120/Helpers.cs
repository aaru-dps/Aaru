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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;

namespace Aaru.Images;

public sealed partial class Alcohol120
{
    static ushort TrackModeToCookedBytesPerSector(TrackMode trackMode)
    {
        return trackMode switch
               {
                   TrackMode.Mode1 or TrackMode.Mode1Alt or TrackMode.Mode2F1 or TrackMode.Mode2F1Alt => 2048,
                   TrackMode.Mode2F2 or TrackMode.Mode2F2Alt                                          => 2324,
                   TrackMode.Mode2                                                                    => 2336,
                   TrackMode.Audio or TrackMode.AudioAlt                                              => 2352,
                   TrackMode.DVD                                                                      => 2048,
                   _                                                                                  => 0
               };
    }

    static TrackType TrackModeToTrackType(TrackMode trackType)
    {
        return trackType switch
               {
                   TrackMode.Mode1 or TrackMode.Mode1Alt     => TrackType.CdMode1,
                   TrackMode.Mode2F1 or TrackMode.Mode2F1Alt => TrackType.CdMode2Form1,
                   TrackMode.Mode2F2 or TrackMode.Mode2F2Alt => TrackType.CdMode2Form2,
                   TrackMode.Mode2                           => TrackType.CdMode2Formless,
                   TrackMode.Audio or TrackMode.AudioAlt     => TrackType.Audio,
                   _                                         => TrackType.Data
               };
    }

    static MediaType MediumTypeToMediaType(MediumType discType) => discType switch
                                                                   {
                                                                       MediumType.CD   => MediaType.CD,
                                                                       MediumType.CDR  => MediaType.CDR,
                                                                       MediumType.CDRW => MediaType.CDRW,
                                                                       MediumType.DVD  => MediaType.DVDROM,
                                                                       MediumType.DVDR => MediaType.DVDR,
                                                                       _               => MediaType.Unknown
                                                                   };

    static MediumType MediaTypeToMediumType(MediaType type)
    {
        return type switch
               {
                   MediaType.CD
                    or MediaType.CDDA
                    or MediaType.CDEG
                    or MediaType.CDG
                    or MediaType.CDI
                    or MediaType.CDMIDI
                    or MediaType.CDPLUS
                    or MediaType.CDROM
                    or MediaType.CDROMXA
                    or MediaType.CDV
                    or MediaType.DTSCD
                    or MediaType.JaguarCD
                    or MediaType.MEGACD
                    or MediaType.PS1CD
                    or MediaType.PS2CD
                    or MediaType.SuperCDROM2
                    or MediaType.SVCD
                    or MediaType.SATURNCD
                    or MediaType.ThreeDO
                    or MediaType.VCD
                    or MediaType.VCDHD
                    or MediaType.NeoGeoCD
                    or MediaType.PCFX
                    or MediaType.CDTV
                    or MediaType.CD32
                    or MediaType.Nuon
                    or MediaType.Playdia
                    or MediaType.Pippin
                    or MediaType.FMTOWNS
                    or MediaType.MilCD
                    or MediaType.VideoNow
                    or MediaType.VideoNowColor
                    or MediaType.VideoNowXp
                    or MediaType.CVD => MediumType.CD,
                   MediaType.CDR                     => MediumType.CDR,
                   MediaType.CDRW or MediaType.CDMRW => MediumType.CDRW,
                   MediaType.DVDR
                    or MediaType.DVDRW
                    or MediaType.DVDPR
                    or MediaType.DVDRDL
                    or MediaType.DVDRWDL
                    or MediaType.DVDPRDL
                    or MediaType.DVDPRWDL => MediumType.DVDR,
                   _ => MediumType.DVD
               };
    }

    static TrackMode TrackTypeToTrackMode(TrackType type) => type switch
                                                             {
                                                                 TrackType.Audio           => TrackMode.Audio,
                                                                 TrackType.CdMode1         => TrackMode.Mode1,
                                                                 TrackType.CdMode2Formless => TrackMode.Mode2,
                                                                 TrackType.CdMode2Form1    => TrackMode.Mode2F1,
                                                                 TrackType.CdMode2Form2    => TrackMode.Mode2F2,
                                                                 _                         => TrackMode.DVD
                                                             };

    static (byte minute, byte second, byte frame) LbaToMsf(ulong sector) =>
        ((byte)((sector + 150) / 75 / 60), (byte)((sector + 150) / 75 % 60), (byte)((sector + 150) % 75));
}