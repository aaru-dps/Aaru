// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Nintendo optical filesystems plugin.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the filesystem used by Nintendo Gamecube and Wii discs</summary>
public sealed partial class NintendoPlugin
{
    static string DiscTypeToString(string discType) => discType switch
                                                       {
                                                           "C" => Localization.Commodore_64_Virtual_Console,
                                                           "D" => Localization.Demo,
                                                           "E" => Localization.Neo_Geo_Virtual_Console,
                                                           "F" => Localization.NES_Virtual_Console,
                                                           "G" => Localization.Gamecube,
                                                           "H" => Localization.Wii_channel,
                                                           "J" => Localization.Super_Nintendo_Virtual_Console,
                                                           "L" => Localization.Master_System_Virtual_Console,
                                                           "M" => Localization.Megadrive_Virtual_Console,
                                                           "N" => Localization.Nintendo_64_Virtual_Console,
                                                           "P" => Localization.
                                                               Promotional_or_TurboGrafx_Virtual_Console,
                                                           "Q" => Localization.TurboGrafx_CD_Virtual_Console,
                                                           "R" => Localization.Wii,
                                                           "S" => Localization.Wii,
                                                           "U" => Localization.Utility,
                                                           "W" => Localization.WiiWare,
                                                           "X" => Localization.MSX_Virtual_Console_or_WiiWare_demo,
                                                           "0" => Localization.Diagnostic,
                                                           "1" => Localization.Diagnostic,
                                                           "4" => Localization.Wii_Backup,
                                                           "_" => Localization.WiiFit,
                                                           _   => string.Format(Localization.unknown_type_0, discType)
                                                       };

    static string RegionCodeToString(string regionCode) => regionCode switch
                                                           {
                                                               "A" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_any_region,
                                                               "D" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_Germany,
                                                               "N" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_USA,
                                                               "E" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_USA,
                                                               "F" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_France,
                                                               "I" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_Italy,
                                                               "J" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_Japan,
                                                               "K" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_Korea,
                                                               "Q" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_Korea,
                                                               "L" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_PAL,
                                                               "M" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_PAL,
                                                               "P" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_PAL,
                                                               "R" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_Russia,
                                                               "S" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_Spain,
                                                               "T" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_Taiwan,
                                                               "U" => Localization.
                                                                   NintendoPlugin_RegionCodeToString_Australia,
                                                               _ => string.Format(
                                                                   Localization.
                                                                       NintendoPlugin_RegionCodeToString_unknown_region_code_0,
                                                                   regionCode)
                                                           };

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    static string PublisherCodeToString(string publisherCode) => publisherCode switch
                                                                 {
                                                                     "01" => "Nintendo",
                                                                     "08" => "CAPCOM",
                                                                     "41" => "Ubisoft",
                                                                     "4F" => "Eidos",
                                                                     "51" => "Acclaim",
                                                                     "52" => "Activision",
                                                                     "5D" => "Midway",
                                                                     "5G" => "Hudson",
                                                                     "64" => "LucasArts",
                                                                     "69" => "Electronic Arts",
                                                                     "6S" => "TDK Mediactive",
                                                                     "8P" => "SEGA",
                                                                     "A4" => "Mirage Studios",
                                                                     "AF" => "Namco",
                                                                     "B2" => "Bandai",
                                                                     "DA" => "Tomy",
                                                                     "EM" => "Konami",
                                                                     "70" => "Atari",
                                                                     "4Q" => "Disney Interactive",
                                                                     "GD" => "Square Enix",
                                                                     "7D" => "Sierra",
                                                                     _ => string.Format(
                                                                         Localization.Unknown_publisher_0,
                                                                         publisherCode)
                                                                 };

    static string PartitionTypeToString(uint type) => type switch
                                                      {
                                                          0 => Localization.data,
                                                          1 => Localization.update,
                                                          2 => Localization.channel,
                                                          _ => string.Format(
                                                              Localization.unknown_partition_type_0, type)
                                                      };
}