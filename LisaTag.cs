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

namespace DiscImageChef.Decoders
{
    public static class LisaTag
    {
        /// <summary>
        /// LisaOS tag as stored on Apple Profile and FileWare disks (20 bytes)
        /// </summary>
        public struct ProfileTag
        {
            /// <summary>0x00, Lisa OS version number</summary>
            public ushort version;
            /// <summary>0x02 bits 7 to 6, kind of info in this block</summary>
            public byte kind;
            /// <summary>0x02 bits 5 to 0, reserved</summary>
            public byte reserved;
            /// <summary>0x03, disk volume number</summary>
            public byte volume;
            /// <summary>0x04, file ID</summary>
            public short fileID;
            /// <summary>
            /// 0x06 bit 7, checksum valid?
            /// </summary>
            public bool validChk;
            /// <summary>
            /// 0x06 bits 6 to 0, used bytes in block
            /// </summary>
            public ushort usedBytes;
            /// <summary>
            /// 0x08, 3 bytes, absolute page number
            /// </summary>
            public uint absPage;
            /// <summary>
            /// 0x0B, checksum of data
            /// </summary>
            public byte checksum;
            /// <summary>
            /// 0x0C, relative page number
            /// </summary>
            public ushort relPage;
            /// <summary>
            /// 0x0E, 3 bytes, next block, 0xFFFFFF if it's last block
            /// </summary>
            public uint nextBlock;
            /// <summary>
            /// 0x11, 3 bytes, previous block, 0xFFFFFF if it's first block
            /// </summary>
            public uint prevBlock;

            /// <summary>On-memory value for easy first block search.</summary>
            public bool isFirst;
            /// <summary>On-memory value for easy last block search.</summary>
            public bool isLast;
}

        /// <summary>
        /// LisaOS tag as stored on Priam DataTower disks (24 bytes)
        /// </summary>
        public struct PriamTag
        {
            /// <summary>0x00, Lisa OS version number</summary>
            public ushort version;
            /// <summary>0x02 bits 7 to 6, kind of info in this block</summary>
            public byte kind;
            /// <summary>0x02 bits 5 to 0, reserved</summary>
            public byte reserved;
            /// <summary>0x03, disk volume number</summary>
            public byte volume;
            /// <summary>0x04, file ID</summary>
            public short fileID;
            /// <summary>
            /// 0x06 bit 7, checksum valid?
            /// </summary>
            public bool validChk;
            /// <summary>
            /// 0x06 bits 6 to 0, used bytes in block
            /// </summary>
            public ushort usedBytes;
            /// <summary>
            /// 0x08, 3 bytes, absolute page number
            /// </summary>
            public uint absPage;
            /// <summary>
            /// 0x0B, checksum of data
            /// </summary>
            public byte checksum;
            /// <summary>
            /// 0x0C, relative page number
            /// </summary>
            public ushort relPage;
            /// <summary>
            /// 0x0E, 3 bytes, next block, 0xFFFFFF if it's last block
            /// </summary>
            public uint nextBlock;
            /// <summary>
            /// 0x11, 3 bytes, previous block, 0xFFFFFF if it's first block
            /// </summary>
            public uint prevBlock;
            /// <summary>
            /// 0x14, disk size
            /// </summary>
            public uint diskSize;

            /// <summary>On-memory value for easy first block search.</summary>
            public bool isFirst;
            /// <summary>On-memory value for easy last block search.</summary>
            public bool isLast;
        }

        /// <summary>
        /// LisaOS tag as stored on Apple Sony disks (12 bytes)
        /// </summary>
        public struct SonyTag
        {
            /// <summary>0x00, Lisa OS version number</summary>
            public ushort version;
            /// <summary>0x02 bits 7 to 6, kind of info in this block</summary>
            public byte kind;
            /// <summary>0x02 bits 5 to 0, reserved</summary>
            public byte reserved;
            /// <summary>0x03, disk volume number</summary>
            public byte volume;
            /// <summary>0x04, file ID</summary>
            public short fileID;
            /// <summary>
            /// 0x06, relative page number
            /// </summary>
            public ushort relPage;
            /// <summary>
            /// 0x08, 3 bytes, next block, 0x7FF if it's last block, 0x8000 set if block is valid
            /// </summary>
            public ushort nextBlock;
            /// <summary>
            /// 0x0A, 3 bytes, previous block, 0x7FF if it's first block
            /// </summary>
            public ushort prevBlock;

            /// <summary>On-memory value for easy first block search.</summary>
            public bool isFirst;
            /// <summary>On-memory value for easy last block search.</summary>
            public bool isLast;
        }

        public static SonyTag? DecodeSonyTag(byte[] tag)
        {
            if(tag == null || tag.Length != 12)
                return null;

            SonyTag snTag = new SonyTag();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            snTag.version = BigEndianBitConverter.ToUInt16(tag, 0);
            snTag.kind = (byte)((tag[2] & 0xC0) >> 6);
            snTag.reserved = (byte)(tag[2] & 0x3F);
            snTag.volume = tag[3];
            snTag.fileID = BigEndianBitConverter.ToInt16(tag, 4);
            snTag.relPage = BigEndianBitConverter.ToUInt16(tag, 6);
            snTag.nextBlock = (ushort)(BigEndianBitConverter.ToUInt16(tag, 8) & 0x7FF);
            snTag.prevBlock = (ushort)(BigEndianBitConverter.ToUInt16(tag, 10) & 0x7FF);

            snTag.isLast = snTag.nextBlock == 0x7FF;
            snTag.isFirst = snTag.prevBlock == 0x7FF;

            return snTag;
        }

        public static ProfileTag? DecodeProfileTag(byte[] tag)
        {
            if(tag == null || tag.Length != 20)
                return null;

            ProfileTag phTag = new ProfileTag();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] tmp = new byte[4];

            phTag.version = BigEndianBitConverter.ToUInt16(tag, 0);
            phTag.kind = (byte)((tag[2] & 0xC0) >> 6);
            phTag.reserved = (byte)(tag[2] & 0x3F);
            phTag.volume = tag[3];
            phTag.fileID = BigEndianBitConverter.ToInt16(tag, 4);
            phTag.validChk |= (tag[6] & 0x80) == 0x80;
            phTag.usedBytes = (ushort)(BigEndianBitConverter.ToUInt16(tag, 6) & 0x7FFF);

            tmp[0] = 0x00;
            tmp[1] = tag[8];
            tmp[2] = tag[9];
            tmp[3] = tag[10];
            phTag.absPage = BigEndianBitConverter.ToUInt32(tmp, 0);

            phTag.checksum = tag[11];
            phTag.relPage = BigEndianBitConverter.ToUInt16(tag, 12);

            tmp[0] = 0x00;
            tmp[1] = tag[14];
            tmp[2] = tag[15];
            tmp[3] = tag[16];
            phTag.nextBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

            tmp[0] = 0x00;
            tmp[1] = tag[17];
            tmp[2] = tag[18];
            tmp[3] = tag[19];
            phTag.prevBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

            phTag.isLast = phTag.nextBlock == 0xFFFFFF;
            phTag.isFirst = phTag.prevBlock == 0xFFFFFF;

            return phTag;
        }

        public static PriamTag? DecodePriamTag(byte[] tag)
        {
            if(tag == null || tag.Length != 24)
                return null;

            PriamTag pmTag = new PriamTag();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] tmp = new byte[4];

            pmTag.version = BigEndianBitConverter.ToUInt16(tag, 0);
            pmTag.kind = (byte)((tag[2] & 0xC0) >> 6);
            pmTag.reserved = (byte)(tag[2] & 0x3F);
            pmTag.volume = tag[3];
            pmTag.fileID = BigEndianBitConverter.ToInt16(tag, 4);
            pmTag.validChk |= (tag[6] & 0x80) == 0x80;
            pmTag.usedBytes = (ushort)(BigEndianBitConverter.ToUInt16(tag, 6) & 0x7FFF);

            tmp[0] = 0x00;
            tmp[1] = tag[8];
            tmp[2] = tag[9];
            tmp[3] = tag[10];
            pmTag.absPage = BigEndianBitConverter.ToUInt32(tmp, 0);

            pmTag.checksum = tag[11];
            pmTag.relPage = BigEndianBitConverter.ToUInt16(tag, 12);

            tmp[0] = 0x00;
            tmp[1] = tag[14];
            tmp[2] = tag[15];
            tmp[3] = tag[16];
            pmTag.nextBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

            tmp[0] = 0x00;
            tmp[1] = tag[17];
            tmp[2] = tag[18];
            tmp[3] = tag[19];
            pmTag.prevBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

            pmTag.diskSize = BigEndianBitConverter.ToUInt32(tag, 20);

            pmTag.isLast = pmTag.nextBlock == 0xFFFFFF;
            pmTag.isFirst = pmTag.prevBlock == 0xFFFFFF;

            return pmTag;
        }

        public static PriamTag? DecodeTag(byte[] tag)
        {
            if(tag == null)
                return null;

            PriamTag pmTag = new PriamTag();

            switch(tag.Length)
            {
                case 12:
                    SonyTag? snTag = DecodeSonyTag(tag);

                    if(snTag == null)
                        return null;

                    pmTag = new PriamTag();
                    pmTag.absPage = 0;
                    pmTag.checksum = 0;
                    pmTag.diskSize = 0;
                    pmTag.fileID = snTag.Value.fileID;
                    pmTag.kind = snTag.Value.kind;
                    pmTag.nextBlock = snTag.Value.nextBlock;
                    pmTag.prevBlock = snTag.Value.prevBlock;
                    pmTag.relPage = snTag.Value.relPage;
                    pmTag.reserved = snTag.Value.reserved;
                    pmTag.usedBytes = 0;
                    pmTag.validChk = false;
                    pmTag.version = snTag.Value.version;
                    pmTag.volume = snTag.Value.volume;
                    pmTag.isFirst = snTag.Value.isFirst;
                    pmTag.isLast = snTag.Value.isLast;

                    return pmTag;
                case 20:
                    ProfileTag? phTag = DecodeProfileTag(tag);

                    if(phTag == null)
                        return null;

                    pmTag = new PriamTag();
                    pmTag.absPage = phTag.Value.absPage;
                    pmTag.checksum = phTag.Value.checksum;
                    pmTag.diskSize = 0;
                    pmTag.fileID = phTag.Value.fileID;
                    pmTag.kind = phTag.Value.kind;
                    pmTag.nextBlock = phTag.Value.nextBlock;
                    pmTag.prevBlock = phTag.Value.prevBlock;
                    pmTag.relPage = phTag.Value.relPage;
                    pmTag.reserved = phTag.Value.reserved;
                    pmTag.usedBytes = phTag.Value.usedBytes;
                    pmTag.validChk = phTag.Value.validChk;
                    pmTag.version = phTag.Value.version;
                    pmTag.volume = phTag.Value.volume;
                    pmTag.isFirst = phTag.Value.isFirst;
                    pmTag.isLast = phTag.Value.isLast;

                    return pmTag;
                case 24:
                    return DecodePriamTag(tag);
                default:
                    return null;
            }
        }
    }
}

