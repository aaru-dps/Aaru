// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
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

// ReSharper disable UnusedMember.Local

using System;

namespace Aaru.Filesystems;

public sealed partial class FAT
{
    const uint FSINFO_SIGNATURE1 = 0x41615252;
    const uint FSINFO_SIGNATURE2 = 0x61417272;
    const uint FSINFO_SIGNATURE3 = 0xAA550000;
    /// <summary>Directory finishes</summary>
    const byte DIRENT_FINISHED = 0x00;
    /// <summary>Deleted directory entry</summary>
    const byte DIRENT_DELETED = 0xE5;
    /// <summary>Minimum allowed value in name/extension</summary>
    const byte DIRENT_MIN = 0x20;
    /// <summary>Value used instead of <see cref="DIRENT_FINISHED" /> for first name character</summary>
    const byte DIRENT_E5 = 0x05;
    /// <summary>Entry points to self or parent directory</summary>
    const byte DIRENT_SUBDIR = 0x2E;
    const uint   FAT32_MASK      = 0x0FFFFFFF;
    const uint   FAT32_END_MASK  = 0xFFFFFF8;
    const uint   FAT32_FORMATTED = 0xFFFFFF6;
    const uint   FAT32_BAD       = 0xFFFFFF7;
    const uint   FAT32_RESERVED  = 0xFFFFFF0;
    const ushort FAT16_END_MASK  = 0xFFF8;
    const ushort FAT16_FORMATTED = 0xFFF6;
    const ushort FAT16_BAD       = 0xFFF7;
    const ushort FAT16_RESERVED  = 0xFFF0;
    const ushort FAT12_END_MASK  = 0xFF8;
    const ushort FAT12_FORMATTED = 0xFF6;
    const ushort FAT12_BAD       = 0xFF7;
    const ushort FAT12_RESERVED  = 0xFF0;
    const byte   LFN_ERASED      = 0x80;
    const byte   LFN_LAST        = 0x40;
    const byte   LFN_MASK        = 0x1F;
    const ushort EADATA_MAGIC    = 0x4445;
    const ushort EASCTR_MAGIC    = 0x4145;
    const ushort EA_UNUSED       = 0xFFFF;
    const ushort EAT_BINARY      = 0xFFFE;
    const ushort EAT_ASCII       = 0xFFFD;
    const ushort EAT_BITMAP      = 0xFFFB;
    const ushort EAT_METAFILE    = 0xFFFA;
    const ushort EAT_ICON        = 0xFFF9;
    const ushort EAT_EA          = 0xFFEE;
    const ushort EAT_MVMT        = 0xFFDF;
    const ushort EAT_MVST        = 0xFFDE;
    const ushort EAT_ASN1        = 0xFFDD;
    const string FAT32_EA_TAIL   = " EA. SF";

    const string FS_TYPE_FAT_PLUS = "fatplus";
    const string FS_TYPE_FAT32    = "fat32";
    const string FS_TYPE_FAT16    = "fat16";
    const string FS_TYPE_FAT12    = "fat12";

    readonly (string hash, string name)[] _knownBootHashes =
    [
        ("b639b4d5b25f63560e3b34a3a0feb732aa65486f", "Amstrad MS-DOS 3.20 (8-sector floppy)"),
        ("9311151f13f7611b1431593da05ddd3153370574", "Amstrad MS-DOS 3.20 (Spanish)"),
        ("55eda6a9b955f5199020e6b56a6954fa6fcb7dc6", "AT&T MS-DOS 2.11"),
        ("d5e10822977efa96e4fbaec2b268ca008d74fe6f", "Atari TOS"), ("17f11a12b96899d2a4976d889cef160502167f2d", "BeOS"),
        ("d0e31673028fcfcea38dff71a7be13669aa20b8d", "Compaq MS-DOS 3.30"),
        ("3aa4ce2fa6f9a297b5b15aaef930401af369fcbc", "Compaq MS-DOS 3.30 (8-sector floppy)"),
        ("8f1d33520343f35034aa3ce47e4180b10e960b43", "Compaq MS-DOS 3.30 (8-sector floppy)"),
        ("2f4011ae0670ff3aff2bdd412a4651f255b600a9", "Compaq MS-DOS 3.31"),
        ("afc3fb751089a52c9f0bd37098d3137d32ab4982", "Concurrent DOS 6.0"),
        ("c25e2d93d3b8bf9870043bf9d12580ef07f5375e", "CrossDOS"),
        ("43fb2afa1ab3102b3f8d0fe901b652a5b0a973e1", "DR-DOS >=7.02"),
        ("92d83b7e9e3bd4b73c6b98f8c6434206eac7212f", "DR-DOS 3.40"),
        ("6a7aba05f91c5a7108edc5d5ccc9dac0ebf5dd28", "DR-DOS 3.40"),
        ("715e78bb1e38b56452dd1c15db8c096dc506eea3", "DR-DOS 3.41"),
        ("01e46cb2bc6d65ddd89ba28d254baf563d8fc609", "DR-DOS 5.00"),
        ("dd297f159eef8a61f5eec638ce7318a098fa26f4", "DR-DOS 5.00"),
        ("f590e53ae7f9caec5dba93241e557710be61b6fe", "DR-DOS 6.00"),
        ("630e4aaf230f15eb2d09985f294815b6bc50384c", "DR-DOS 6.00"),
        ("8851459816d714f53b9c469472e51460ebd44b98", "DR-DOS 7.03"),
        ("cf24388b61eb1b137f2bb8a4c319e7461d460b72", "DR-DOS 7.03"),
        ("26bf8efe8368e598397b1f79d635faccf5ca4f87", "DR-DOS 8.00"),
        ("36ddd6bf8686801f5a2a3cbd4656afa175cabdfc", "DR-DOS 8.00"),
        ("9ac09781e4090d9ba5e1a31816b4ebfa6b54f39e", "DR-DOS 8.00"),
        ("f5d68f26abec8392ac23716581c4ab1d6e8456a2", "eComStation"),
        ("e2a852db8c3eb3d86ca86a706186a9dd54cdc815", "Epson MS-DOS 3.10"),
        ("74675d158dd0f6b5983bd30dda7018a814bd34dd", "Epson MS-DOS 3.20"),
        ("683a04f34714555df5e862f709f9abc7de51488e", "Epson MS-DOS 5.00 (PC-98)"),
        ("1b062df94fc576af069e40603bf2558000a2ca10", "FreeBSD, NetBSD, Mac OS X"),
        ("33c8e306b3a51e09668fd60f099450019a1238ea", "FreeBSD, NetBSD, Mac OS X"),
        ("ca386b1cefaf964a49192c2cd08077aff09b82af", "FreeDOS"),
        ("26033e0db1ee4f439f07077b790e189d1b77688c", "HP MS-DOS 3.20"),
        ("66867cd665e0e87c32de0bcb721ecfe91a551bc5", "mkfs.vfat"),
        ("48d4811dbea5d724803017d6d45a49d604982b7b", "mkfs.vfat"),
        ("02d615c5fb68bc49766bf89777dd36accb1428b7", "MS-DOS >=4.01, PC-DOS >=5.00 (8-sector floppy)"),
        ("cffb1dc01bf9f533ba63d949c359c9ae97944c9b", "MS-DOS >=5.00"),
        ("bd099151f2f9f8b3815eef7d9fed90abead52d97", "MS-DOS 3.21"),
        ("f338eeff68f4e03ce489eb165e18fd94f8a0c63e", "MS-DOS 3.30A"),
        ("0eb0c789e141d59d91f075ef14d63fd4255aeac2", "MS-DOS 3.30A, 5.00, 6.xx (8-sector floppy)"),
        ("7aa06595490f92e542965b43805eaa0d6da9c86c", "MS-DOS 3.31"),
        ("00401ca66900d7defcbc3d794654d1ba2376e83d", "MS-DOS 3.31"),
        ("00e39a27d9b36e88f2b0caaa1959a8e17223bf31", "MS-DOS 3.31 (8-sector floppy)"),
        ("1e74fbad5948582247280b116e7175b5a16bcede", "MS-DOS 3.31 (8-sector floppy)"),
        ("1cfb2cc3a34c8164b8f5051c118643b23a60f4d0", "MS-DOS 4.01"),
        ("d370551c8aca9cfcd964e2a1235071329f93cc03", "Multiuser DOS 7.22r4"),
        ("7106ea11dd0b9a39649cbb4f6314a0fa1241da38", "NEC >=MS-DOS 5.00 (PC-98)"),
        ("30d4f5f54215af0fc69236594e52b94989b35c21", "NEC MS-DOS 3.30 (PC-98)"),
        ("eae42777f562eb81ed624f0c0479347ba11158c9", "Novell DOS 7.00"),
        ("c67a7d0bab94a960cca8720d476f5317c960b2fb", "Novell DOS 7.00"),
        ("0ed508c71bcf1418a1701b9267ddf166e7993b6b", "Olivetti MS-DOS 3.10"),
        ("3e1a3f22973d9f2d15f9a053204c2f7b72de00a9", "OS/2 1.00"),
        ("4acb13943f21a266f9eb110969980481783c41c4", "OS/2 1.10"),
        ("cc7b32236c76d34edefdac3ca6a7be8e26163cea", "OS/2 1.20, 1.30"),
        ("05c6706189fa0532ea83a7484b88d1dcba63e167", "OS/2 2.00"),
        ("eefb4383b3e2b05f7f7e0051d58f4617a4f39e42", "OS/2 2.1x, Warp 3"),
        ("b1d9666ae8781958242da27d319e73aa2dda6805", "OS/2 Warp 4"),
        ("661d92970ab0f81cec453dc7584e0debdd1b4928", "PC-DOS >=5.00"),
        ("78522b303fe5752f3cb37eabb6cb54e6cb0cd276", "PC-DOS 2.xx"),
        ("277edfefdc3f05a219f9378076227c4126b6c8ef", "PC-DOS 3.00"),
        ("f72ef6ff4c90a170bc8cdd1b01e8d98235c16a3b", "PC-DOS 3.10"),
        ("cb6b6c1bc024e025710288da652d0d93527a71db", "PC-DOS 3.30"),
        ("3b844e2a411182958c2d9e6ee17c6d4fff18bd73", "PC-DOS 4.00"),
        ("45e82fcff4c6f8a9c31418323bc063011f5730e5", "PCExchange 2.0"),
        ("707849fd75b6a52fd219c3cebe060ecb23c40fc7", "SCO OpenServer"),
        ("9f7f146b513b00ff9e8b5b927aee24025c3f3fb8", "Toshiba MS-DOS 3.30"),
        ("30eafc45a4606a7b840dcd5899dfb977a837c835", "Toshiba MS-DOS 4.01"),
        ("5695a9a69637bd4d4eaa531b976d6f3b71e8d4ad", "Toshiba MS-DOS 4.01"),
        ("036a59138d620c16e8d0dba45af4dec4ae1376f7", "Windows 10"),
        ("3dd2941b79f0f6644b3a973c7d81e64e434c0b70", "Windows 2000"),
        ("cd1e2fced2e49825df23c08a3d1e280c1cf468a7", "Windows 2000"),
        ("c20c6e706be97091768da8654fffc2f3c0431318", "Windows 95"),
        ("cf750cc0d2d52251a1a0fb9f2568fed3ff81717a", "Windows 95 >=OSR2"),
        ("48865a298d4bbe73f89c4de10153e16250c1a9ae", "Windows 95 >=OSR2"),
        ("1d2df8f3b1b336fc4aa1c6e49b21956194884b41", "Windows 98 (Spanish)"),
        ("6e6fb4a3ea034415d716b1f81217ffecf78813c3", "Windows 98 (Spanish)"),
        ("9b5e6be09200145a6ed7c22d14b1e6b4de2f362b", "Windows 98 (Spanish)"),
        ("3cea1921d29fcd3343d36c090cb3e3dba926781d", "Windows 98, Me"),
        ("037f9c8caed602d93c88f7e9d8f13a732b3ada76", "Windows NT"),
        ("a63806bfe11140c873082318dd4da834068be327", "Windows Vista"),
        ("8f024b3d501c39ee6e3f8ca28173ad6a780d3eb0", "Windows Vista, 8, 10"),
        ("d3e93f8b82ef250db216037d827a4896dc97d2be", "TracerST"), // OEM ID: "TracerST"
        //("b741f85ef40288ccc8887de1f6e849009097e1c9", "Norton Utilities"), // OEM ID: "IBM PNCI", need to confirm
        ("c49b275537ac7237cac64d83f34d2024ae0ca96a", "Windows NT (Spanish)"), // Need to check Windows >= 2000 (Spanish)
        //("a48b0e4b696317eed829e960d1aa576562a4f185", "TracerST"), // Unknown OEM ID, apparently Tracer, unconfirmed
        ("fe477972602ba76658ff7143859045b3c4036ca5",
         "iomega"), // OEM ID: "SHIPDISK", contains timedate on boot code may not be unique
        ("ef79a1f33e5237827eb812dda548f0e4e916d815", "GEOS"), // OEM ID: "GEOWORKS"
        ("8524587ee91494cc51cc2c9d07453e84be0cdc33", "Hero Soft v1.10"),
        ("681a0d9d662ba368e6acb0d0bf602e1f56411144", "Human68k 2.00"),
        ("91e2b47c3cb46611249e4daa283a68ba21ba596a", "Human68k 2.00")
    ];

#region Nested type: BpbKind

    enum BpbKind
    {
        None,
        Hardcoded,
        Atari,
        Msx,
        Dos2,
        Dos3,
        Dos32,
        Dos33,
        ShortExtended,
        Extended,
        ShortFat32,
        LongFat32,
        Andos,
        Apricot,
        DecRainbow,
        Human
    }

#endregion

#region Nested type: CaseInfo

    [Flags]
    enum CaseInfo : byte
    {
        /// <summary>FASTFAT.SYS indicator that basename is lowercase</summary>
        LowerCaseBasename = 0x08,
        /// <summary>FASTFAT.SYS indicator that extension is lowercase</summary>
        LowerCaseExtension = 0x10,
        AllLowerCase = 0x18,
        /// <summary>FAT32.IFS &lt; 0.97 indicator for normal EAs present</summary>
        NormalEaOld = 0xEA,
        /// <summary>FAT32.IFS &lt; 0.97 indicator for critical EAs present</summary>
        CriticalEaOld = 0xEC,
        /// <summary>FAT32.IFS &gt;= 0.97 indicator for normal EAs present</summary>
        NormalEa = 0x40,
        /// <summary>FAT32.IFS &gt;= 0.97 indicator for critical EAs present</summary>
        CriticalEa = 0x80
    }

#endregion

#region Nested type: EaFlags

    [Flags]
    enum EaFlags : uint
    {
        Normal   = 0,
        Critical = 1
    }

#endregion

#region Nested type: FatAttributes

    [Flags]
    enum FatAttributes : byte
    {
        ReadOnly     = 0x01,
        Hidden       = 0x02,
        System       = 0x04,
        VolumeLabel  = 0x08,
        Subdirectory = 0x10,
        Archive      = 0x20,
        Device       = 0x40,
        Reserved     = 0x80,
        LFN          = 0x0F
    }

#endregion

#region Nested type: Namespace

    enum Namespace
    {
        Dos,
        Nt,
        Lfn,
        Os2,
        Ecs,
        Human
    }

#endregion
}