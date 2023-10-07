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
            0x101F61CE => Localization.SIS_Platform_UID_UIQ_21,
            0x101F6300 => Localization.SIS_Platform_UID_UIQ_30,
            0x101F6F87 => Localization.SIS_Platform_UID_Nokia_7650,
            0x101F6F88 => Localization.SIS_Platform_UID_Series_60_1st_Edition,
            0x101F795F => Localization.SIS_Platform_UID_Series_60_v1_0,
            0x101F7960 => Localization.SIS_Platform_UID_Series_60_2nd_Edition,
            0x101F7961 => Localization.SIS_Platform_UID_Series_60_3rd_Edition,
            0x101F7962 => Localization.SIS_Platform_UID_Nokia_3650,
            0x101F7963 => Localization.SIS_Platform_UID_Nokia_6600,
            0x101F7964 => Localization.SIS_Platform_UID_Nokia_6630,
            0x101F80BE => Localization.SIS_Platform_UID_SonyEricsson_P80x,
            0x101F8201 => Localization.SIS_Platform_UID_Series_60_v1_1,
            0x101F8202 => Localization.SIS_Platform_UID_Series_60_1st_Edition_Feature_Pack_1,
            0x101F8A64 => Localization.SIS_Platform_UID_Nokia_N_Gage,
            0x101F8DDB => Localization.SIS_Platform_UID_Nokia_9500,
            0x101F8ED1 => Localization.SIS_Platform_UID_Nokia_9300,
            0x101F8ED2 => Localization.SIS_Platform_UID_Series_80_2nd_edition,
            0x101F9071 => Localization.SIS_Platform_UID_Siemens_SX1,
            0x101F9115 => Localization.SIS_Platform_UID_Series_60_2nd_Edition_Feature_Pack_1,
            0x101FB3F4 => Localization.SIS_Platform_UID_Nokia_6260,
            0x101FBB35 => Localization.SIS_Platform_UID_SonyEricsson_P90x,
            0x101FBE05 => Localization.SIS_Platform_UID_Nokia_7710_Series_90,
            0x101FD5DB => Localization.SIS_Platform_UID_Nokia_7610,
            0x101FD5DC => Localization.SIS_Platform_UID_Nokia_6670,
            0x101f617b => Localization.SIS_Platform_UID_UIQ_20,
            0x10200BAB => Localization.SIS_Platform_UID_Series_60_2nd_Edition_Feature_Pack_2,
            0x10200F97 => Localization.SIS_Platform_UID_Nokia_3230,
            0x10200F98 => Localization.SIS_Platform_UID_Nokia_N90,
            0x10200F9A => Localization.SIS_Platform_UID_Nokia_N70,
            0x10200f99 => Localization.SIS_Platform_UID_Nokia_6680,
            0x1020216B => Localization.SIS_Platform_UID_Nokia_6620,
            0x102032BD => Localization.SIS_Platform_UID_Series_60_2nd_Edition_Feature_Pack_3,
            0x102032BE => Localization.SIS_Platform_UID_Series_60_3rd_Edition_Feature_Pack_1,
            0x102078CF => Localization.SIS_Platform_UID_Nokia_6682,
            0x102078D0 => Localization.SIS_Platform_UID_Nokia_6681,
            0x102752AE => Localization.SIS_Platform_UID_Series_60_3rd_Edition_Feature_Pack_2,
            0x1028315F => Localization.SIS_Platform_UID_Series_60_5th_Edition,
            0x200005F8 => Localization.SIS_Platform_UID_Nokia_3250,
            0x200005F9 => Localization.SIS_Platform_UID_Nokia_N80,
            0x200005FA => Localization.SIS_Platform_UID_Nokia_N92,
            0x200005FB => Localization.SIS_Platform_UID_Nokia_N73,
            0x200005FC => Localization.SIS_Platform_UID_Nokia_N91,
            0x200005FF => Localization.SIS_Platform_UID_Nokia_N71,
            0x20001856 => Localization.SIS_Platform_UID_Nokia_E60,
            0x20001857 => Localization.SIS_Platform_UID_Nokia_E70,
            0x20001858 => Localization.SIS_Platform_UID_Nokia_E61,
            0x20022E6D => Localization.SIS_Platform_UID_Symbian_3,
            0x2003A678 => Localization.SIS_Platform_UID_Nokia_Belle,
            _          => string.Format(Localization.SIS_Platform_UID_0, uid)
        };
}