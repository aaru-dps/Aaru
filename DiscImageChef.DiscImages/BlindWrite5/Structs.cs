// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for BlindWrite 5 disc images.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Decoders.SCSI.MMC;

namespace DiscImageChef.DiscImages
{
    public partial class BlindWrite5
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Bw5Header
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] signature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public uint[] unknown1;
            public ProfileNumber profile;
            public ushort        sessions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public uint[] unknown2;
            [MarshalAs(UnmanagedType.U1, SizeConst = 3)]
            public bool mcnIsValid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            public byte[] mcn;
            public ushort unknown3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] unknown4;
            public ushort pmaLen;
            public ushort atipLen;
            public ushort cdtLen;
            public ushort cdInfoLen;
            public uint   bcaLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public uint[] unknown5;
            public uint dvdStrLen;
            public uint dvdInfoLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] unknown6;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] manufacturer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] product;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] revision;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] vendor;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] volumeId;
            public uint mode2ALen;
            public uint unkBlkLen;
            public uint dataLen;
            public uint sessionsLen;
            public uint dpmLen;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Bw5DataFile
        {
            public uint Type;
            public uint Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public uint[] Unknown1;
            public uint Offset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] Unknown2;
            public int    StartLba;
            public int    Sectors;
            public uint   FilenameLen;
            public byte[] FilenameBytes;
            public uint   Unknown3;

            public string Filename;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Bw5TrackDescriptor
        {
            public Bw5TrackType type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] unknown1;
            public uint               unknown2;
            public Bw5TrackSubchannel subchannel;
            public byte               unknown3;
            public byte               ctl;
            public byte               adr;
            public byte               point;
            public byte               tno;
            public byte               min;
            public byte               sec;
            public byte               frame;
            public byte               zero;
            public byte               pmin;
            public byte               psec;
            public byte               pframe;
            public byte               unknown5;
            public uint               pregap;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] unknown6;
            public int startLba;
            public int sectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] unknown7;
            public uint   session;
            public ushort unknown8;
            // Seems to be only on non DVD track descriptors
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] unknown9;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Bw5SessionDescriptor
        {
            public ushort               Sequence;
            public byte                 Entries;
            public byte                 Unknown;
            public int                  Start;
            public int                  End;
            public ushort               FirstTrack;
            public ushort               LastTrack;
            public Bw5TrackDescriptor[] Tracks;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DataFileCharacteristics
        {
            public IFilter             FileFilter;
            public string              FilePath;
            public TrackSubchannelType Subchannel;
            public long                SectorSize;
            public int                 StartLba;
            public int                 Sectors;
        }
    }
}