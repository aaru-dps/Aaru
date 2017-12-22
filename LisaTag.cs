// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : LisaTag.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Apple Lisa tags.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;

namespace DiscImageChef.Decoders
{
    [SuppressMessage("ReSharper", "MemberCanBeInternal")]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class LisaTag
    {
        /// <summary>
        /// LisaOS tag as stored on Apple Profile and FileWare disks (20 bytes)
        /// </summary>
        public struct ProfileTag
        {
            /// <summary>0x00, Lisa OS version number</summary>
            public ushort Version;
            /// <summary>0x02 bits 7 to 6, kind of info in this block</summary>
            public byte Kind;
            /// <summary>0x02 bits 5 to 0, reserved</summary>
            public byte Reserved;
            /// <summary>0x03, disk volume number</summary>
            public byte Volume;
            /// <summary>0x04, file ID</summary>
            public short FileId;
            /// <summary>
            /// 0x06 bit 7, checksum valid?
            /// </summary>
            public bool ValidChk;
            /// <summary>
            /// 0x06 bits 6 to 0, used bytes in block
            /// </summary>
            public ushort UsedBytes;
            /// <summary>
            /// 0x08, 3 bytes, absolute page number
            /// </summary>
            public uint AbsPage;
            /// <summary>
            /// 0x0B, checksum of data
            /// </summary>
            public byte Checksum;
            /// <summary>
            /// 0x0C, relative page number
            /// </summary>
            public ushort RelPage;
            /// <summary>
            /// 0x0E, 3 bytes, next block, 0xFFFFFF if it's last block
            /// </summary>
            public uint NextBlock;
            /// <summary>
            /// 0x11, 3 bytes, previous block, 0xFFFFFF if it's first block
            /// </summary>
            public uint PrevBlock;

            /// <summary>On-memory value for easy first block search.</summary>
            public bool IsFirst;
            /// <summary>On-memory value for easy last block search.</summary>
            public bool IsLast;
        }

        /// <summary>
        /// LisaOS tag as stored on Priam DataTower disks (24 bytes)
        /// </summary>
        public struct PriamTag
        {
            /// <summary>0x00, Lisa OS version number</summary>
            public ushort Version;
            /// <summary>0x02 bits 7 to 6, kind of info in this block</summary>
            public byte Kind;
            /// <summary>0x02 bits 5 to 0, reserved</summary>
            public byte Reserved;
            /// <summary>0x03, disk volume number</summary>
            public byte Volume;
            /// <summary>0x04, file ID</summary>
            public short FileId;
            /// <summary>
            /// 0x06 bit 7, checksum valid?
            /// </summary>
            public bool ValidChk;
            /// <summary>
            /// 0x06 bits 6 to 0, used bytes in block
            /// </summary>
            public ushort UsedBytes;
            /// <summary>
            /// 0x08, 3 bytes, absolute page number
            /// </summary>
            public uint AbsPage;
            /// <summary>
            /// 0x0B, checksum of data
            /// </summary>
            public byte Checksum;
            /// <summary>
            /// 0x0C, relative page number
            /// </summary>
            public ushort RelPage;
            /// <summary>
            /// 0x0E, 3 bytes, next block, 0xFFFFFF if it's last block
            /// </summary>
            public uint NextBlock;
            /// <summary>
            /// 0x11, 3 bytes, previous block, 0xFFFFFF if it's first block
            /// </summary>
            public uint PrevBlock;
            /// <summary>
            /// 0x14, disk size
            /// </summary>
            public uint DiskSize;

            /// <summary>On-memory value for easy first block search.</summary>
            public bool IsFirst;
            /// <summary>On-memory value for easy last block search.</summary>
            public bool IsLast;
        }

        /// <summary>
        /// LisaOS tag as stored on Apple Sony disks (12 bytes)
        /// </summary>
        public struct SonyTag
        {
            /// <summary>0x00, Lisa OS version number</summary>
            public ushort Version;
            /// <summary>0x02 bits 7 to 6, kind of info in this block</summary>
            public byte Kind;
            /// <summary>0x02 bits 5 to 0, reserved</summary>
            public byte Reserved;
            /// <summary>0x03, disk volume number</summary>
            public byte Volume;
            /// <summary>0x04, file ID</summary>
            public short FileId;
            /// <summary>
            /// 0x06, relative page number
            /// </summary>
            public ushort RelPage;
            /// <summary>
            /// 0x08, 3 bytes, next block, 0x7FF if it's last block, 0x8000 set if block is valid
            /// </summary>
            public ushort NextBlock;
            /// <summary>
            /// 0x0A, 3 bytes, previous block, 0x7FF if it's first block
            /// </summary>
            public ushort PrevBlock;

            /// <summary>On-memory value for easy first block search.</summary>
            public bool IsFirst;
            /// <summary>On-memory value for easy last block search.</summary>
            public bool IsLast;
        }

        public static SonyTag? DecodeSonyTag(byte[] tag)
        {
            if(tag == null || tag.Length != 12) return null;

            SonyTag snTag = new SonyTag();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            snTag.Version = BigEndianBitConverter.ToUInt16(tag, 0);
            snTag.Kind = (byte)((tag[2] & 0xC0) >> 6);
            snTag.Reserved = (byte)(tag[2] & 0x3F);
            snTag.Volume = tag[3];
            snTag.FileId = BigEndianBitConverter.ToInt16(tag, 4);
            snTag.RelPage = BigEndianBitConverter.ToUInt16(tag, 6);
            snTag.NextBlock = (ushort)(BigEndianBitConverter.ToUInt16(tag, 8) & 0x7FF);
            snTag.PrevBlock = (ushort)(BigEndianBitConverter.ToUInt16(tag, 10) & 0x7FF);

            snTag.IsLast = snTag.NextBlock == 0x7FF;
            snTag.IsFirst = snTag.PrevBlock == 0x7FF;

            return snTag;
        }

        public static ProfileTag? DecodeProfileTag(byte[] tag)
        {
            if(tag == null || tag.Length != 20) return null;

            ProfileTag phTag = new ProfileTag();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] tmp = new byte[4];

            phTag.Version = BigEndianBitConverter.ToUInt16(tag, 0);
            phTag.Kind = (byte)((tag[2] & 0xC0) >> 6);
            phTag.Reserved = (byte)(tag[2] & 0x3F);
            phTag.Volume = tag[3];
            phTag.FileId = BigEndianBitConverter.ToInt16(tag, 4);
            phTag.ValidChk |= (tag[6] & 0x80) == 0x80;
            phTag.UsedBytes = (ushort)(BigEndianBitConverter.ToUInt16(tag, 6) & 0x7FFF);

            tmp[0] = 0x00;
            tmp[1] = tag[8];
            tmp[2] = tag[9];
            tmp[3] = tag[10];
            phTag.AbsPage = BigEndianBitConverter.ToUInt32(tmp, 0);

            phTag.Checksum = tag[11];
            phTag.RelPage = BigEndianBitConverter.ToUInt16(tag, 12);

            tmp[0] = 0x00;
            tmp[1] = tag[14];
            tmp[2] = tag[15];
            tmp[3] = tag[16];
            phTag.NextBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

            tmp[0] = 0x00;
            tmp[1] = tag[17];
            tmp[2] = tag[18];
            tmp[3] = tag[19];
            phTag.PrevBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

            phTag.IsLast = phTag.NextBlock == 0xFFFFFF;
            phTag.IsFirst = phTag.PrevBlock == 0xFFFFFF;

            return phTag;
        }

        public static PriamTag? DecodePriamTag(byte[] tag)
        {
            if(tag == null || tag.Length != 24) return null;

            PriamTag pmTag = new PriamTag();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] tmp = new byte[4];

            pmTag.Version = BigEndianBitConverter.ToUInt16(tag, 0);
            pmTag.Kind = (byte)((tag[2] & 0xC0) >> 6);
            pmTag.Reserved = (byte)(tag[2] & 0x3F);
            pmTag.Volume = tag[3];
            pmTag.FileId = BigEndianBitConverter.ToInt16(tag, 4);
            pmTag.ValidChk |= (tag[6] & 0x80) == 0x80;
            pmTag.UsedBytes = (ushort)(BigEndianBitConverter.ToUInt16(tag, 6) & 0x7FFF);

            tmp[0] = 0x00;
            tmp[1] = tag[8];
            tmp[2] = tag[9];
            tmp[3] = tag[10];
            pmTag.AbsPage = BigEndianBitConverter.ToUInt32(tmp, 0);

            pmTag.Checksum = tag[11];
            pmTag.RelPage = BigEndianBitConverter.ToUInt16(tag, 12);

            tmp[0] = 0x00;
            tmp[1] = tag[14];
            tmp[2] = tag[15];
            tmp[3] = tag[16];
            pmTag.NextBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

            tmp[0] = 0x00;
            tmp[1] = tag[17];
            tmp[2] = tag[18];
            tmp[3] = tag[19];
            pmTag.PrevBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

            pmTag.DiskSize = BigEndianBitConverter.ToUInt32(tag, 20);

            pmTag.IsLast = pmTag.NextBlock == 0xFFFFFF;
            pmTag.IsFirst = pmTag.PrevBlock == 0xFFFFFF;

            return pmTag;
        }

        public static PriamTag? DecodeTag(byte[] tag)
        {
            if(tag == null) return null;

            PriamTag pmTag;

            switch(tag.Length)
            {
                case 12:
                    SonyTag? snTag = DecodeSonyTag(tag);

                    if(snTag == null) return null;

                    pmTag = new PriamTag();
                    pmTag.AbsPage = 0;
                    pmTag.Checksum = 0;
                    pmTag.DiskSize = 0;
                    pmTag.FileId = snTag.Value.FileId;
                    pmTag.Kind = snTag.Value.Kind;
                    pmTag.NextBlock = snTag.Value.NextBlock;
                    pmTag.PrevBlock = snTag.Value.PrevBlock;
                    pmTag.RelPage = snTag.Value.RelPage;
                    pmTag.Reserved = snTag.Value.Reserved;
                    pmTag.UsedBytes = 0;
                    pmTag.ValidChk = false;
                    pmTag.Version = snTag.Value.Version;
                    pmTag.Volume = snTag.Value.Volume;
                    pmTag.IsFirst = snTag.Value.IsFirst;
                    pmTag.IsLast = snTag.Value.IsLast;

                    return pmTag;
                case 20:
                    ProfileTag? phTag = DecodeProfileTag(tag);

                    if(phTag == null) return null;

                    pmTag = new PriamTag();
                    pmTag.AbsPage = phTag.Value.AbsPage;
                    pmTag.Checksum = phTag.Value.Checksum;
                    pmTag.DiskSize = 0;
                    pmTag.FileId = phTag.Value.FileId;
                    pmTag.Kind = phTag.Value.Kind;
                    pmTag.NextBlock = phTag.Value.NextBlock;
                    pmTag.PrevBlock = phTag.Value.PrevBlock;
                    pmTag.RelPage = phTag.Value.RelPage;
                    pmTag.Reserved = phTag.Value.Reserved;
                    pmTag.UsedBytes = phTag.Value.UsedBytes;
                    pmTag.ValidChk = phTag.Value.ValidChk;
                    pmTag.Version = phTag.Value.Version;
                    pmTag.Volume = phTag.Value.Volume;
                    pmTag.IsFirst = phTag.Value.IsFirst;
                    pmTag.IsLast = phTag.Value.IsLast;

                    return pmTag;
                case 24: return DecodePriamTag(tag);
                default: return null;
            }
        }
    }
}