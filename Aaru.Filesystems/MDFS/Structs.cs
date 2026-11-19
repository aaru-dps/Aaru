// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MiniDisc File System plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using Aaru.Helpers;

namespace Aaru.Filesystems;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public sealed partial class MDFS
{
#region Nested type: VolumeDescriptor

    /// <summary>Volume descriptor, the first sector of the volume management area. All fields are big-endian.</summary>
    /// <remarks>Field names are those printed by Sony's MDfsck.</remarks>
    readonly struct VolumeDescriptor
    {
        /// <summary>0x06, version, 1</summary>
        public readonly byte Version;
        /// <summary>0x10, bytes per logical block, 2048</summary>
        public readonly ushort BlockSize;
        /// <summary>0x12, sectors per MiniDisc cluster, 32</summary>
        public readonly ushort ClusterSize;
        /// <summary>0x14, sectors per allocation unit</summary>
        public readonly ushort AllocSize;
        /// <summary>0x18, allocation units on the disc, counting from sector 0</summary>
        public readonly uint NumAlloc;
        /// <summary>0x1C, recordable allocation units</summary>
        public readonly uint NumRecordable;
        /// <summary>0x20, free allocation units</summary>
        public readonly uint NumAvailable;
        /// <summary>0x24, used allocation units</summary>
        public readonly uint NumUsed;
        /// <summary>0x28, defective allocation units</summary>
        public readonly uint NumDefective;
        /// <summary>0x30, directories, including the root</summary>
        public readonly ushort NumDir;
        /// <summary>0x32, files</summary>
        public readonly ushort NumFile;
        /// <summary>0x34, highest record id issued</summary>
        public readonly uint MaxIdNum;
        /// <summary>0x38, volume attributes</summary>
        public readonly ushort VolAttr;
        /// <summary>0x3C, length in bytes of the volume management area</summary>
        public readonly uint VmaLen;
        /// <summary>0x40, sector of the volume management area, where this descriptor is</summary>
        public readonly uint VmaLoc;
        /// <summary>0x44, first sector of the space bitmap, relative to the management area</summary>
        public readonly ushort VsbLoc;
        /// <summary>0x46, sectors of the space bitmap</summary>
        public readonly ushort VsbNum;
        /// <summary>0x48, first sector of the management table, relative to the management area</summary>
        public readonly ushort MtbLoc;
        /// <summary>0x4A, sectors of the management table</summary>
        public readonly ushort MtbNum;
        /// <summary>0x4C, first extent record block, relative to the management area</summary>
        public readonly ushort ErbLoc;
        /// <summary>0x4E, sectors of extent record blocks</summary>
        public readonly ushort ErbNum;
        /// <summary>0x50, first sector of the root directory, relative to the management area</summary>
        public readonly ushort DrbLoc;
        /// <summary>0x52, sectors of directory records on the volume</summary>
        public readonly ushort DrbNum;
        /// <summary>0x54, root directory length in bytes</summary>
        public readonly uint DirLen;
        /// <summary>0x58, entries in the root directory</summary>
        public readonly ushort NumChild;
        /// <summary>0x5E, character set of the boot identifier</summary>
        public readonly byte BootIdCharset;
        /// <summary>0x60, boot identifier, 32 bytes</summary>
        public readonly byte[] BootId;
        /// <summary>0x80, character set of the volume identifier</summary>
        public readonly byte VolIdCharset;
        /// <summary>0x82, volume identifier (label), 32 bytes</summary>
        public readonly byte[] VolId;
        /// <summary>0xA2, character set of the publisher identifier</summary>
        public readonly byte PubIdCharset;
        /// <summary>0xA4, publisher identifier, 128 bytes</summary>
        public readonly byte[] PubId;
        /// <summary>0x124, character set of the data preparer identifier</summary>
        public readonly byte PrepIdCharset;
        /// <summary>0x126, data preparer identifier, 128 bytes</summary>
        public readonly byte[] PrepId;
        /// <summary>0x1A6, character set of the application identifier</summary>
        public readonly byte ApplIdCharset;
        /// <summary>0x1A8, application identifier, 128 bytes</summary>
        public readonly byte[] ApplId;
        /// <summary>0x280, creation time</summary>
        public readonly uint CrTime;
        /// <summary>0x284, modification time</summary>
        public readonly uint MoTime;
        /// <summary>0x288, effective time</summary>
        public readonly uint EfTime;
        /// <summary>0x28C, expiration time</summary>
        public readonly uint ExTime;

        public VolumeDescriptor(byte[] sector)
        {
            Version       = sector[0x06];
            BlockSize     = BigEndianBitConverter.ToUInt16(sector, 0x10);
            ClusterSize   = BigEndianBitConverter.ToUInt16(sector, 0x12);
            AllocSize     = BigEndianBitConverter.ToUInt16(sector, 0x14);
            NumAlloc      = BigEndianBitConverter.ToUInt32(sector, 0x18);
            NumRecordable = BigEndianBitConverter.ToUInt32(sector, 0x1C);
            NumAvailable  = BigEndianBitConverter.ToUInt32(sector, 0x20);
            NumUsed       = BigEndianBitConverter.ToUInt32(sector, 0x24);
            NumDefective  = BigEndianBitConverter.ToUInt32(sector, 0x28);
            NumDir        = BigEndianBitConverter.ToUInt16(sector, 0x30);
            NumFile       = BigEndianBitConverter.ToUInt16(sector, 0x32);
            MaxIdNum      = BigEndianBitConverter.ToUInt32(sector, 0x34);
            VolAttr       = BigEndianBitConverter.ToUInt16(sector, 0x38);
            VmaLen        = BigEndianBitConverter.ToUInt32(sector, 0x3C);
            VmaLoc        = BigEndianBitConverter.ToUInt32(sector, 0x40);
            VsbLoc        = BigEndianBitConverter.ToUInt16(sector, 0x44);
            VsbNum        = BigEndianBitConverter.ToUInt16(sector, 0x46);
            MtbLoc        = BigEndianBitConverter.ToUInt16(sector, 0x48);
            MtbNum        = BigEndianBitConverter.ToUInt16(sector, 0x4A);
            ErbLoc        = BigEndianBitConverter.ToUInt16(sector, 0x4C);
            ErbNum        = BigEndianBitConverter.ToUInt16(sector, 0x4E);
            DrbLoc        = BigEndianBitConverter.ToUInt16(sector, 0x50);
            DrbNum        = BigEndianBitConverter.ToUInt16(sector, 0x52);
            DirLen        = BigEndianBitConverter.ToUInt32(sector, 0x54);
            NumChild      = BigEndianBitConverter.ToUInt16(sector, 0x58);
            BootIdCharset = sector[0x5E];
            BootId        = sector[0x60..0x80];
            VolIdCharset  = sector[0x80];
            VolId         = sector[0x82..0xA2];
            PubIdCharset  = sector[0xA2];
            PubId         = sector[0xA4..0x124];
            PrepIdCharset = sector[0x124];
            PrepId        = sector[0x126..0x1A6];
            ApplIdCharset = sector[0x1A6];
            ApplId        = sector[0x1A8..0x228];
            CrTime        = BigEndianBitConverter.ToUInt32(sector, 0x280);
            MoTime        = BigEndianBitConverter.ToUInt32(sector, 0x284);
            EfTime        = BigEndianBitConverter.ToUInt32(sector, 0x288);
            ExTime        = BigEndianBitConverter.ToUInt32(sector, 0x28C);
        }

        /// <summary>Is the volume management area mirrored?</summary>
        public bool Mirrored => (VolAttr & VOL_MIRROR) != 0;

        /// <summary>Do extent records use 32-bit fields?</summary>
        public bool WideExtents => (VolAttr & VOL_EXTENT_32) != 0;

        /// <summary>Bytes per allocation unit</summary>
        public uint AllocationUnitBytes => (uint)AllocSize * BLOCK_SIZE;

        /// <summary>Checks the fields a reader depends on</summary>
        public bool IsSane(ulong lba) => Version     == VD_VERSION &&
                                         BlockSize   == BLOCK_SIZE &&
                                         AllocSize   > 0           &&
                                         VmaLoc      == lba        &&
                                         MtbNum      > 0           &&
                                         NumAlloc    > 0;

        /// <summary>Gets an identifier that is not empty</summary>
        public static bool IsEmpty(ReadOnlySpan<byte> field)
        {
            foreach(byte b in field)
                if(b != 0 && b != 0x20)
                    return false;

            return true;
        }
    }

#endregion
}