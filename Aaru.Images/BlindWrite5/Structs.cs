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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Decoders.SCSI.MMC;

namespace Aaru.DiscImages;

public sealed partial class BlindWrite5
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] signature;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly uint[] unknown1;
        public readonly ProfileNumber profile;
        public readonly ushort        sessions;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly uint[] unknown2;
        [MarshalAs(UnmanagedType.U1, SizeConst = 3)]
        public readonly bool mcnIsValid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        public readonly byte[] mcn;
        public readonly ushort unknown3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly uint[] unknown4;
        public readonly ushort pmaLen;
        public readonly ushort atipLen;
        public readonly ushort cdtLen;
        public readonly ushort cdInfoLen;
        public readonly uint   bcaLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly uint[] unknown5;
        public readonly uint dvdStrLen;
        public readonly uint dvdInfoLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] unknown6;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] manufacturer;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] product;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] revision;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] vendor;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] volumeId;
        public readonly uint mode2ALen;
        public readonly uint unkBlkLen;
        public readonly uint dataLen;
        public readonly uint sessionsLen;
        public readonly uint dpmLen;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DataFile
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
    readonly struct TrackDescriptor
    {
        public readonly TrackType type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] unknown1;
        public readonly uint            unknown2;
        public readonly TrackSubchannel subchannel;
        public readonly byte            unknown3;
        public readonly byte            ctl;
        public readonly byte            adr;
        public readonly byte            point;
        public readonly byte            tno;
        public readonly byte            min;
        public readonly byte            sec;
        public readonly byte            frame;
        public readonly byte            zero;
        public readonly byte            pmin;
        public readonly byte            psec;
        public readonly byte            pframe;
        public readonly byte            unknown5;
        public readonly uint            pregap;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly uint[] unknown6;
        public readonly int startLba;
        public readonly int sectors;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly uint[] unknown7;
        public readonly uint   session;
        public readonly ushort unknown8;

        // Seems to be only on non DVD track descriptors
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly uint[] unknown9;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SessionDescriptor
    {
        public ushort            Sequence;
        public byte              Entries;
        public byte              Unknown;
        public int               Start;
        public int               End;
        public ushort            FirstTrack;
        public ushort            LastTrack;
        public TrackDescriptor[] Tracks;
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
        public uint                Offset;
    }
}