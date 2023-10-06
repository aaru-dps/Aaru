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
//     Contains structures for BlindWrite 4 disc images.
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

using System.Runtime.InteropServices;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Images;

public sealed partial class BlindWrite4
{
#region Nested type: Header

    struct Header
    {
        public byte[] Signature;
        public uint   Unknown1;
        public ulong  Timestamp;
        public uint   VolumeIdLength;
        public byte[] VolumeIdBytes;
        public uint   SysIdLength;
        public byte[] SysIdBytes;
        public uint   CommentsLength;
        public byte[] CommentsBytes;
        public uint   TrackDescriptors;
        public uint   DataFileLength;
        public byte[] DataFileBytes;
        public uint   SubchannelFileLength;
        public byte[] SubchannelFileBytes;
        public uint   Unknown2;
        public byte   Unknown3;
        public byte[] Unknown4;

        // On memory only
    #pragma warning disable 649
        public string  VolumeIdentifier;
        public string  SystemIdentifier;
        public string  Comments;
        public IFilter DataFilter;
        public IFilter SubchannelFilter;
        public string  DataFile;
        public string  SubchannelFile;
    #pragma warning restore 649
    }

#endregion

#region Nested type: TrackDescriptor

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TrackDescriptor
    {
        public uint   filenameLen;
        public byte[] filenameBytes;
        public uint   offset;
        public byte   subchannel;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] unknown1;
        public uint      unknown2;
        public byte      unknown3;
        public byte      session;
        public byte      unknown4;
        public byte      adrCtl;
        public byte      unknown5;
        public TrackType trackMode;
        public byte      unknown6;
        public byte      point;
        public uint      unknown7;
        public uint      unknown8;

        // Seems to be used to adjust the offset according to the pregap
        public uint   pregapOffsetAdjustment;
        public uint   unknown10;
        public ushort unknown11;
        public uint   lastSector;
        public byte   unknown12;
        public int    pregap;
        public int    startSector;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] unknown13;
        public uint   titleLen;
        public byte[] titleBytes;
        public uint   performerLen;
        public byte[] performerBytes;
        public uint   unkStrLen1;
        public byte[] unkStrBytes1;
        public uint   unkStrLen2;
        public byte[] unkStrBytes2;
        public uint   unkStrLen3;
        public byte[] unkStrBytes3;
        public uint   unkStrLen4;
        public byte[] unkStrBytes4;
        public uint   discIdLen;
        public byte[] discIdBytes;
        public uint   unkStrLen5;
        public byte[] unkStrBytes5;
        public uint   unkStrLen6;
        public byte[] unkStrBytes6;
        public uint   unkStrLen7;
        public byte[] unkStrBytes7;
        public uint   unkStrLen8;
        public byte[] unkStrBytes8;
        public uint   unkStrLen9;
        public byte[] unkStrBytes9;
        public uint   unkStrLen10;
        public byte[] unkStrBytes10;
        public uint   unkStrLen11;
        public byte[] unkStrBytes11;
        public uint   isrcLen;
        public byte[] isrcBytes;

        // On memory only
        public string filename;
        public string title;
        public string performer;
        public string unkString1;
        public string unkString2;
        public string unkString3;
        public string unkString4;
        public string discId;
        public string unkString5;
        public string unkString6;
        public string unkString7;
        public string unkString8;
        public string unkString9;
        public string unkString10;
        public string unkString11;
        public string isrcUpc;
    }

#endregion
}