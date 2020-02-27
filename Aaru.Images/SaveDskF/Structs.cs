// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for IBM SaveDskF disk images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.DiscImages
{
    public partial class SaveDskF
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SaveDskFHeader
        {
            /// <summary>0x00 magic number</summary>
            public ushort magic;
            /// <summary>0x02 media type from FAT</summary>
            public ushort mediaType;
            /// <summary>0x04 bytes per sector</summary>
            public ushort sectorSize;
            /// <summary>0x06 sectors per cluster - 1</summary>
            public byte clusterMask;
            /// <summary>0x07 log2(cluster / sector)</summary>
            public byte clusterShift;
            /// <summary>0x08 reserved sectors</summary>
            public ushort reservedSectors;
            /// <summary>0x0A copies of FAT</summary>
            public byte fatCopies;
            /// <summary>0x0B entries in root directory</summary>
            public ushort rootEntries;
            /// <summary>0x0D first cluster</summary>
            public ushort firstCluster;
            /// <summary>0x0F clusters present in image</summary>
            public ushort clustersCopied;
            /// <summary>0x11 sectors per FAT</summary>
            public byte sectorsPerFat;
            /// <summary>0x12 sector number of root directory</summary>
            public ushort rootDirectorySector;
            /// <summary>0x14 sum of all image bytes</summary>
            public uint checksum;
            /// <summary>0x18 cylinders</summary>
            public ushort cylinders;
            /// <summary>0x1A heads</summary>
            public ushort heads;
            /// <summary>0x1C sectors per track</summary>
            public ushort sectorsPerTrack;
            /// <summary>0x1E always zero</summary>
            public uint padding;
            /// <summary>0x22 sectors present in image</summary>
            public ushort sectorsCopied;
            /// <summary>0x24 offset to comment</summary>
            public ushort commentOffset;
            /// <summary>0x26 offset to data</summary>
            public ushort dataOffset;
        }
    }
}