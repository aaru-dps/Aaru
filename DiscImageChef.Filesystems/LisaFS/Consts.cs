// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple Lisa filesystem constants.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        const byte LisaFSv1 = 0x0E;
        const byte LisaFSv2 = 0x0F;
        const byte LisaFSv3 = 0x11;
        /// <summary>Maximum string size in LisaFS</summary>
        const uint E_NAME = 32;
        const ushort FILEID_FREE = 0x0000;
        const ushort FILEID_BOOT = 0xAAAA;
        const ushort FILEID_LOADER = 0xBBBB;
        const ushort FILEID_MDDF = 0x0001;
        const ushort FILEID_BITMAP = 0x0002;
        const ushort FILEID_SRECORD = 0x0003;
        /// <summary>"Catalog file"</summary>
        const ushort FILEID_DIRECTORY = 0x0004;
        const short FILEID_BOOT_SIGNED = -21846;
        const short FILEID_LOADER_SIGNED = -17477;
        const ushort FILEID_ERASED = 0x7FFF;
        const ushort FILEID_MAX = FILEID_ERASED;

        enum FileType : byte
        {
            Undefined = 0,
            MDDFile = 1,
            RootCat = 2,
            FreeList = 3,
            BadBlocks = 4,
            SysData = 5,
            Spool = 6,
            Exec = 7,
            UserCat = 8,
            Pipe = 9,
            BootFile = 10,
            SwapData = 11,
            SwapCode = 12,
            RamAP = 13,
            UserFile = 14,
            KilledObject = 15
        }
    }
}

