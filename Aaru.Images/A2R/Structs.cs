// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for A2R flux images.
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
// Copyright © 2011-2024 Rebecca Wallander
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class A2R
{
#region Nested type: A2RHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct A2RHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] signature;
        public byte version;
        public byte highBitTest; // Should always be 0xFF
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] lineTest; // Should always be 0x0A 0x0D 0x0A
    }

#endregion

#region Nested type: ChunkHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ChunkHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] chunkId;
        public uint chunkSize;
    }

#endregion

#region Nested type: InfoChunkV2

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InfoChunkV2
    {
        public ChunkHeader header;
        public byte        version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] creator;
        public A2RDiskType diskType;
        public byte        writeProtected;
        public byte        synchronized;
    }

#endregion

#region Nested type: InfoChunkV3

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InfoChunkV3
    {
        public ChunkHeader header;
        public byte        version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] creator;
        public A2rDriveType driveType;
        public byte         writeProtected;
        public byte         synchronized;
        public byte         hardSectorCount;
    }

#endregion

#region Nested type: RwcpChunkHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RwcpChunkHeader
    {
        public ChunkHeader header;
        public byte        version;
        public uint        resolution;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public byte[] reserved;
    }

#endregion

#region Nested type: SlvdChunkHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SlvdChunkHeader
    {
        public ChunkHeader header;
        public byte        version;
        public uint        resolution;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public byte[] reserved;
    }

#endregion

#region Nested type: StreamCapture

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StreamCapture
    {
        public byte   mark;
        public byte   captureType;
        public ushort location;
        public byte   numberOfIndexSignals;
        public uint[] indexSignals;
        public uint   captureDataSize;
        public long   dataOffset;
        public uint   resolution;
        public uint   head;
        public ushort track;
        public byte   subTrack;
    }

#endregion

#region Nested type: TrackHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrackHeader
    {
        public byte   mark;
        public ushort location;
        public byte   mirrorDistanceOutward;
        public byte   mirrorDistanceInward;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] reserved;
        public byte   numberOfIndexSignals;
        public uint[] indexSignals;
        public uint   fluxDataSize;
    }

#endregion
}