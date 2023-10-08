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

    static string DecodeMachineUid(uint uid) =>
        uid switch
        {
            0x200005f8 => "UID_NOKIA_3250",
            0x20024104 => "UID_NOKIA_5228",
            0x20023763 => "UID_NOKIA_5230",
            0x20023764 => "UID_NOKIA_5230a",
            0x2002376b => "UID_NOKIA_5230_NURON",
            0x20024105 => "UID_NOKIA_5235",
            0x2002bf93 => "UID_NOKIA_5250",
            0x2000da5a => "UID_NOKIA_5320",
            0x20000602 => "UID_NOKIA_5500",
            0x2001de9d => "UID_NOKIA_5530",
            0x2001de9e => "UID_NOKIA_5530_chinese",
            0x2000da61 => "UID_NOKIA_5630",
            0x20002d7c => "UID_NOKIA_5700",
            0x20014dd3 => "UID_NOKIA_5730",
            0x2000da56 => "UID_NOKIA_5800",
            0x20002d7b => "UID_NOKIA_6110",
            0x20002d7e => "UID_NOKIA_6120",
            0x2000da55 => "UID_NOKIA_6124",
            0x2000da54 => "UID_NOKIA_6210",
            0x2000da52 => "UID_NOKIA_6220",
            0x2000da53 => "UID_NOKIA_6220_cn",
            0x20000606 => "UID_NOKIA_6290",
            0x2000da57 => "UID_NOKIA_6650",
            0x2001de9b => "UID_NOKIA_6700",
            0x20014dd1 => "UID_NOKIA_6710",
            0x20014dcd => "UID_NOKIA_6720",
            0x2001de9a => "UID_NOKIA_6730",
            0x200227e2 => "UID_NOKIA_6760",
            0x200227e6 => "UID_NOKIA_6788",
            0x2001de98 => "UID_NOKIA_6790",
            0x20024107 => "UID_NOKIA_C5_00",
            0x20029a75 => "UID_NOKIA_C5_01",
            0x2002bf91 => "UID_NOKIA_C6_00",
            0x2002376a => "UID_NOKIA_C6_01",
            0x2002bf92 => "UID_NOKIA_C7_00",
            0x20002495 => "UID_NOKIA_E50",
            0x20002498 => "UID_NOKIA_E51",
            0x20014dcc => "UID_NOKIA_E52",
            0x20014dcf => "UID_NOKIA_E55",
            0x20001856 => "UID_NOKIA_E60",
            0x20001858 => "UID_NOKIA_E61",
            0x20002d7f => "UID_NOKIA_E61i",
            0x20001859 => "UID_NOKIA_E62",
            0x200025c3 => "UID_NOKIA_E63",
            0x20000604 => "UID_NOKIA_E65",
            0x2000249c => "UID_NOKIA_E66",
            0x2002bf96 => "UID_NOKIA_E7_00",
            0x20001857 => "UID_NOKIA_E70",
            0x2000249b => "UID_NOKIA_E71",
            0x20014dd8 => "UID_NOKIA_E71x",
            0x20014dd0 => "UID_NOKIA_E72",
            0x20029a6d => "UID_NOKIA_E73",
            0x2000249d => "UID_NOKIA_E75",
            0x20002496 => "UID_NOKIA_E90",
            0x20024100 => "UID_NOKIA_E5_00",
            0x10200f9a => "UID_NOKIA_N70",
            0x200005ff => "UID_NOKIA_N71",
            0x200005fb => "UID_NOKIA_N73",
            0x200005fe => "UID_NOKIA_N75",
            0x2000060a => "UID_NOKIA_N76",
            0x20000601 => "UID_NOKIA_N77",
            0x20002d81 => "UID_NOKIA_N78",
            0x2000da64 => "UID_NOKIA_N79",
            0x20029a73 => "UID_NOKIA_N8_00",
            0x200005f9 => "UID_NOKIA_N80",
            0x20002d83 => "UID_NOKIA_N81",
            0x20002d85 => "UID_NOKIA_N82",
            0x20002d86 => "UID_NOKIA_N85",
            0x20014dd2 => "UID_NOKIA_N86",
            0x200005fc => "UID_NOKIA_N91",
            0x200005fa => "UID_NOKIA_N92",
            0x20000600 => "UID_NOKIA_N93",
            0x20000605 => "UID_NOKIA_N93i",
            0x2000060b => "UID_NOKIA_N95",
            0x20002d84 => "UID_NOKIA_N95_8GB",
            0x20002d82 => "UID_NOKIA_N96",
            0x20014ddd => "UID_NOKIA_N97",
            0x20014dde => "UID_NOKIA_N97a",
            0x20023766 => "UID_NOKIA_N97_mini",
            0x20029a76 => "UID_NOKIA_X5_00",
            0x20024101 => "UID_NOKIA_X5_01",
            0x200227dd => "UID_NOKIA_X6",
            0x20008610 => "UID_SAMSUNG_I400",
            0x2000a677 => "UID_SAMSUNG_I450",
            0x20003abd => "UID_SAMSUNG_I520",
            0x2000a678 => "UID_SAMSUNG_I550",
            0x2000c51c => "UID_SAMSUNG_I560",
            0x2000a679 => "UID_SAMSUNG_I590",
            0x2000c51f => "UID_SAMSUNG_I7110",
            0x2000c51e => "UID_SAMSUNG_I8510",
            0x2000c520 => "UID_SAMSUNG_I8910",
            0x2001f0a1 => "UID_SONYERICSSON_SATIO",
            0x10274bf9 => "UID_SONYERICSSON_M600",
            0x10274bfa => "UID_SONYERICSSON_W950",
            0x20002e6a => "UID_SONYERICSSON_W960",
            0x1020e285 => "UID_SONYERICSSON_P990",
            0x20002e69 => "UID_SONYERICSSON_P1",
            0x2000cc70 => "UID_SONYERICSSON_G700",
            0x2000cc6c => "UID_SONYERICSSON_G900",
            0x20024eec => "UID_SONYERICSSON_VIVAZ_U5i",
            0x20024eed => "UID_SONYERICSSON_VIVAZ_U8",
            0x1027400d => "UID_MOTOROLA_Z8",
            0x101ff809 => "UID_LG_KT610",
            0x10200f97 => "UID_NOKIA_3230",
            0x101f466a => "UID_NOKIA_3650",
            0x101f8c19 => "UID_NOKIA_NGAGE",
            0x101fb2b1 => "UID_NOKIA_NGAGE_QD",
            0x101fb3f4 => "UID_NOKIA_6260",
            0x101fb3dd => "UID_NOKIA_6600",
            0x101f3ee3 => "UID_NOKIA_6620",
            0x101fbb55 => "UID_NOKIA_6630",
            0x101fb3f3 => "UID_NOKIA_6670",
            0x10200f99 => "UID_NOKIA_6680",
            0x10200f9c => "UID_NOKIA_6681",
            0x10200f9b => "UID_NOKIA_6682",
            0x101f4fc3 => "UID_NOKIA_7650",
            0x10005e33 => "UID_NOKIA_92XX",
            0x101f8ddb => "UID_NOKIA_9x00",
            0x1020e048 => "UID_NOKIA_9300i",
            0x10200f98 => "UID_NOKIA_N90",
            0x101fbe09 => "UID_NOKIA_7710",
            0x101fa031 => "UID_SENDO_X",
            0x101f9071 => "UID_SIEMENS_SX1",
            0x101fe7b7 => "SAMSUNG_SGH_D730",
            0x101f408b => "UID_SONYERICSSON_P800",
            0x101fb2ae => "UID_SONYERICSSON_P900",
            0x10200ac6 => "UID_SONYERICSSON_P910",
            0x101f6b26 => "UID_MOTOROLA_A9XX",
            0x101f6b27 => "UID_MOTOROLA_A1000",
            _          => $"{uid:X8}"
        };
}