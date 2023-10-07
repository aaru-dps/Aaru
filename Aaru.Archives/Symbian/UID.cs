// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Symbian.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Symbian plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Symbian installer (.sis) packages and shows information.
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

namespace Aaru.Archives;

public sealed partial class Symbian
{
    static string DecodePlatformUid(uint uid) =>
        uid switch
        {
            0x101F6F88 => Localization.SIS_Platform_UID_Series_60_1st_Edition,
            0x101F8202 => Localization.SIS_Platform_UID_Series_60_1st_Edition_Feature_Pack_1,
            0x101F7960 => Localization.SIS_Platform_UID_Series_60_2nd_Edition,
            0x101F9115 => Localization.SIS_Platform_UID_Series_60_2nd_Edition_Feature_Pack_1,
            0x10200BAB => Localization.SIS_Platform_UID_Series_60_2nd_Edition_Feature_Pack_2,
            0x102032BD => Localization.SIS_Platform_UID_Series_60_2nd_Edition_Feature_Pack_3,
            0x101F7961 => Localization.SIS_Platform_UID_Series_60_3rd_Edition,
            0x102032BE => Localization.SIS_Platform_UID_Series_60_3rd_Edition_Feature_Pack_1,
            0x102752AE => Localization.SIS_Platform_UID_Series_60_3rd_Edition_Feature_Pack_2,
            0x1028315F => Localization.SIS_Platform_UID_Series_60_5th_Edition,
            0x20022E6D => Localization.SIS_Platform_UID_Symbian_3,
            0x2003A678 => Localization.SIS_Platform_UID_Nokia_Belle,
            _          => string.Format(Localization.SIS_Platform_UID_0, uid)
        };
}