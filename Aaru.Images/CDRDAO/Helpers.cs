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
//     Contains helpers for cdrdao cuesheets (toc/bin).
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
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

public sealed partial class Cdrdao
{
    static ushort CdrdaoTrackTypeToCookedBytesPerSector(string trackType)
    {
        return trackType switch
               {
                   CDRDAO_TRACK_TYPE_MODE1 or CDRDAO_TRACK_TYPE_MODE2_FORM1 or CDRDAO_TRACK_TYPE_MODE1_RAW => 2048,
                   CDRDAO_TRACK_TYPE_MODE2_FORM2                                                           => 2324,
                   CDRDAO_TRACK_TYPE_MODE2 or CDRDAO_TRACK_TYPE_MODE2_MIX or CDRDAO_TRACK_TYPE_MODE2_RAW   => 2336,
                   CDRDAO_TRACK_TYPE_AUDIO                                                                 => 2352,
                   _                                                                                       => 0
               };
    }

    static TrackType CdrdaoTrackTypeToTrackType(string trackType)
    {
        return trackType switch
               {
                   CDRDAO_TRACK_TYPE_MODE1 or CDRDAO_TRACK_TYPE_MODE1_RAW => TrackType.CdMode1,
                   CDRDAO_TRACK_TYPE_MODE2_FORM1                          => TrackType.CdMode2Form1,
                   CDRDAO_TRACK_TYPE_MODE2_FORM2                          => TrackType.CdMode2Form2,
                   CDRDAO_TRACK_TYPE_MODE2 or CDRDAO_TRACK_TYPE_MODE2_MIX or CDRDAO_TRACK_TYPE_MODE2_RAW => TrackType
                      .CdMode2Formless,
                   CDRDAO_TRACK_TYPE_AUDIO => TrackType.Audio,
                   _                       => TrackType.Data
               };
    }

    static (byte minute, byte second, byte frame) LbaToMsf(ulong sector) =>
        ((byte)(sector / 75 / 60), (byte)(sector / 75 % 60), (byte)(sector % 75));

    static string GetTrackMode(Track track)
    {
        switch(track.Type)
        {
            case TrackType.Audio when track.RawBytesPerSector == 2352:
                return CDRDAO_TRACK_TYPE_AUDIO;
            case TrackType.Data:
                return CDRDAO_TRACK_TYPE_MODE1;
            case TrackType.CdMode1 when track.RawBytesPerSector == 2352:
                return CDRDAO_TRACK_TYPE_MODE1_RAW;
            case TrackType.CdMode2Formless when track.RawBytesPerSector != 2352:
                return CDRDAO_TRACK_TYPE_MODE2;
            case TrackType.CdMode2Form1 when track.RawBytesPerSector != 2352:
                return CDRDAO_TRACK_TYPE_MODE2_FORM1;
            case TrackType.CdMode2Form2 when track.RawBytesPerSector != 2352:
                return CDRDAO_TRACK_TYPE_MODE2_FORM2;
            case TrackType.CdMode2Formless when track.RawBytesPerSector == 2352:
            case TrackType.CdMode2Form1 when track.RawBytesPerSector    == 2352:
            case TrackType.CdMode2Form2 when track.RawBytesPerSector    == 2352:
                return CDRDAO_TRACK_TYPE_MODE2_RAW;
            default:
                return CDRDAO_TRACK_TYPE_MODE1;
        }
    }
}