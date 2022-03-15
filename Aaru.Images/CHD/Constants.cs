// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for MAME Compressed Hunks of Data disk images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

using System.Diagnostics.CodeAnalysis;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Chd
{
    /// <summary>"GDDD"</summary>
    const uint HARD_DISK_METADATA = 0x47444444;
    /// <summary>"IDNT"</summary>
    const uint HARD_DISK_IDENT_METADATA = 0x49444E54;
    /// <summary>"KEY "</summary>
    const uint HARD_DISK_KEY_METADATA = 0x4B455920;
    /// <summary>"CIS "</summary>
    const uint PCMCIA_CIS_METADATA = 0x43495320;
    /// <summary>"CHCD"</summary>
    const uint CDROM_OLD_METADATA = 0x43484344;
    /// <summary>"CHTR"</summary>
    const uint CDROM_TRACK_METADATA = 0x43485452;
    /// <summary>"CHT2"</summary>
    const uint CDROM_TRACK_METADATA2 = 0x43485432;
    /// <summary>"CHGT"</summary>
    const uint GDROM_OLD_METADATA = 0x43484754;
    /// <summary>"CHGD"</summary>
    const uint GDROM_METADATA = 0x43484744;
    /// <summary>"AVAV"</summary>
    const uint AV_METADATA = 0x41564156;
    /// <summary>"AVLD"</summary>
    const uint AV_LASER_DISC_METADATA = 0x41564C44;

    const string REGEX_METADATA_HDD =
        @"CYLS:(?<cylinders>\d+),HEADS:(?<heads>\d+),SECS:(?<sectors>\d+),BPS:(?<bps>\d+)";
    const string REGEX_METADATA_CDROM =
        @"TRACK:(?<track>\d+) TYPE:(?<track_type>\S+) SUBTYPE:(?<sub_type>\S+) FRAMES:(?<frames>\d+)";
    const string REGEX_METADATA_CDROM2 =
        @"TRACK:(?<track>\d+) TYPE:(?<track_type>\S+) SUBTYPE:(?<sub_type>\S+) FRAMES:(?<frames>\d+) PREGAP:(?<pregap>\d+) PGTYPE:(?<pgtype>\S+) PGSUB:(?<pgsub>\S+) POSTGAP:(?<postgap>\d+)";
    const string REGEX_METADATA_GDROM =
        @"TRACK:(?<track>\d+) TYPE:(?<track_type>\S+) SUBTYPE:(?<sub_type>\S+) FRAMES:(?<frames>\d+) PAD:(?<pad>\d+) PREGAP:(?<pregap>\d+) PGTYPE:(?<pgtype>\S+) PGSUB:(?<pgsub>\S+) POSTGAP:(?<postgap>\d+)";

    // ReSharper disable InconsistentNaming
    const string TRACK_TYPE_MODE1        = "MODE1";
    const string TRACK_TYPE_MODE1_2K     = "MODE1/2048";
    const string TRACK_TYPE_MODE1_RAW    = "MODE1_RAW";
    const string TRACK_TYPE_MODE1_RAW_2K = "MODE1/2352";
    const string TRACK_TYPE_MODE2        = "MODE2";
    const string TRACK_TYPE_MODE2_2K     = "MODE2/2336";
    const string TRACK_TYPE_MODE2_F1     = "MODE2_FORM1";
    const string TRACK_TYPE_MODE2_F1_2K  = "MODE2/2048";
    const string TRACK_TYPE_MODE2_F2     = "MODE2_FORM2";
    const string TRACK_TYPE_MODE2_F2_2K  = "MODE2/2324";
    const string TRACK_TYPE_MODE2_FM     = "MODE2_FORM_MIX";
    const string TRACK_TYPE_MODE2_RAW    = "MODE2_RAW";
    const string TRACK_TYPE_MODE2_RAW_2K = "MODE2/2352";
    const string TRACK_TYPE_AUDIO        = "AUDIO";
    // ReSharper restore InconsistentNaming

    const string SUB_TYPE_COOKED = "RW";
    const string SUB_TYPE_RAW    = "RW_RAW";
    const string SUB_TYPE_NONE   = "NONE";

    const int MAX_CACHE_SIZE = 16777216;
}