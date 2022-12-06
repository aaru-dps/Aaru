// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple Macintosh File System structures.
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

using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable 169

namespace Aaru.Filesystems
{
    // Information from Inside Macintosh Volume II
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public sealed partial class AppleMFS
    {
        /// <summary>Master Directory Block, should be at offset 0x0400 bytes in volume</summary>
        struct MasterDirectoryBlock
        {
            /// <summary>0x000, Signature, 0xD2D7</summary>
            public ushort drSigWord;
            /// <summary>0x002, Volume creation date</summary>
            public uint drCrDate;
            /// <summary>0x006, Volume last backup date</summary>
            public uint drLsBkUp;
            /// <summary>0x00A, Volume attributes</summary>
            public AppleCommon.VolumeAttributes drAtrb;
            /// <summary>0x00C, Volume number of files</summary>
            public ushort drNmFls;
            /// <summary>0x00E, First directory sector</summary>
            public ushort drDirSt;
            /// <summary>0x010, Length of directory in sectors</summary>
            public ushort drBlLen;
            /// <summary>0x012, Volume allocation blocks</summary>
            public ushort drNmAlBlks;
            /// <summary>0x014, Size of allocation blocks</summary>
            public uint drAlBlkSiz;
            /// <summary>0x018, Number of bytes to allocate</summary>
            public uint drClpSiz;
            /// <summary>0x01C, First allocation block in block map</summary>
            public ushort drAlBlSt;
            /// <summary>0x01E. Next unused file number</summary>
            public uint drNxtFNum;
            /// <summary>0x022, Number of unused allocation blocks</summary>
            public ushort drFreeBks;
            /// <summary>0x024, Length of volume name</summary>
            public byte drVNSiz;
            /// <summary>0x025, Characters of volume name</summary>
            public string drVN;
        }

        [Flags]
        enum FileFlags : byte
        {
            Locked = 0x01, Used = 0x80
        }

        struct FileEntry
        {
            /// <summary>0x00, Entry flags</summary>
            public FileFlags flFlags;
            /// <summary>0x01, Version number</summary>
            public byte flTyp;
            /// <summary>0x02, FinderInfo</summary>
            public AppleCommon.FInfo flUsrWds;
            /// <summary>0x12, file ID</summary>
            public uint flFlNum;
            /// <summary>0x16, first allocation block of data fork</summary>
            public ushort flStBlk;
            /// <summary>0x18, logical end-of-file of data fork</summary>
            public uint flLgLen;
            /// <summary>0x1C, physical end-of-file of data fork</summary>
            public uint flPyLen;
            /// <summary>0x20, first allocation block of resource fork</summary>
            public ushort flRStBlk;
            /// <summary>0x22, logical end-of-file of resource fork</summary>
            public uint flRLgLen;
            /// <summary>0x26, physical end-of-file of resource fork</summary>
            public uint flRPyLen;
            /// <summary>0x2A, date and time of creation</summary>
            public uint flCrDat;
            /// <summary>0x2E, date and time of last modification</summary>
            public uint flMdDat;
            /// <summary>0x32, file name prefixed with length</summary>
            public byte[] flNam;
        }
    }
}