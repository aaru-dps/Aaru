// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : U.C.S.D. Pascal filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     U.C.S.D. Pascal filesystem structures.
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

namespace DiscImageChef.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public partial class PascalPlugin : Filesystem
    {
        struct PascalVolumeEntry
        {
            /// <summary>0x00, first block of volume entry</summary>
            public short firstBlock;
            /// <summary>0x02, last block of volume entry</summary>
            public short lastBlock;
            /// <summary>0x04, entry type</summary>
            public PascalFileKind entryType;
            /// <summary>0x06, volume name</summary>
            public byte[] volumeName;
            /// <summary>0x0E, block in volume</summary>
            public short blocks;
            /// <summary>0x10, files in volume</summary>
            public short files;
            /// <summary>0x12, dummy</summary>
            public short dummy;
            /// <summary>0x14, last booted</summary>
            public short lastBoot;
            /// <summary>0x16, tail to make record same size as <see cref="PascalFileEntry"/></summary>
            public int tail;
        }

        struct PascalFileEntry
        {
            /// <summary>0x00, first block of file</summary>
            public short firstBlock;
            /// <summary>0x02, last block of file</summary>
            public short lastBlock;
            /// <summary>0x04, entry type</summary>
            public PascalFileKind entryType;
            /// <summary>0x06, file name</summary>
            public byte[] filename;
            /// <summary>0x16, bytes used in last block</summary>
            public short lastBytes;
            /// <summary>0x18, modification time</summary>
            public short mtime;
        }
    }
}

