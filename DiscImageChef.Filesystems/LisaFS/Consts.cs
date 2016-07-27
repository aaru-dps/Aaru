// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;

namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        const byte LisaFSv1 = 0x0E;
        const byte LisaFSv2 = 0x0F;
        const byte LisaFSv3 = 0x11;
        /// <summary>Maximum string size in LisaFS</summary>
        const uint E_NAME = 32;
        const UInt16 FILEID_FREE = 0x0000;
        const UInt16 FILEID_BOOT = 0xAAAA;
        const UInt16 FILEID_LOADER = 0xBBBB;
        const UInt16 FILEID_MDDF = 0x0001;
        const UInt16 FILEID_BITMAP = 0x0002;
        const UInt16 FILEID_SRECORD = 0x0003;
        /// <summary>"Catalog file"</summary>
        const UInt16 FILEID_DIRECTORY = 0x0004;
        const Int16 FILEID_BOOT_SIGNED = -21846;
        const Int16 FILEID_LOADER_SIGNED = -17477;
        const UInt16 FILEID_ERASED = 0x7FFF;
        const UInt16 FILEID_MAX = FILEID_ERASED;

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

