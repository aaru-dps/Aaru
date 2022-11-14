// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Decoders;

using System;
using System.Diagnostics.CodeAnalysis;
using Aaru.Helpers;

/// <summary>Represents a Lisa Office 7/7 sector tag</summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal"), SuppressMessage("ReSharper", "NotAccessedField.Global"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"),
 SuppressMessage("ReSharper", "StructMemberCanBeMadeReadOnly")]
public static class LisaTag
{
    /// <summary>Decodes tag from a 3.5" Sony micro-floppy</summary>
    /// <param name="tag">Byte array containing raw tag data</param>
    /// <returns>Decoded tag in Sony's format</returns>
    public static SonyTag? DecodeSonyTag(byte[] tag)
    {
        if(tag        == null ||
           tag.Length != 12)
            return null;

        var snTag = new SonyTag
        {
            Version   = BigEndianBitConverter.ToUInt16(tag, 0),
            Kind      = (byte)((tag[2] & 0xC0) >> 6),
            Reserved  = (byte)(tag[2] & 0x3F),
            Volume    = tag[3],
            FileId    = BigEndianBitConverter.ToInt16(tag, 4),
            RelPage   = BigEndianBitConverter.ToUInt16(tag, 6),
            NextBlock = (ushort)(BigEndianBitConverter.ToUInt16(tag, 8)  & 0x7FF),
            PrevBlock = (ushort)(BigEndianBitConverter.ToUInt16(tag, 10) & 0x7FF)
        };

        snTag.IsLast  = snTag.NextBlock == 0x7FF;
        snTag.IsFirst = snTag.PrevBlock == 0x7FF;

        return snTag;
    }

    /// <summary>Decodes tag from a Profile</summary>
    /// <param name="tag">Byte array containing raw tag data</param>
    /// <returns>Decoded tag in Profile's format</returns>
    public static ProfileTag? DecodeProfileTag(byte[] tag)
    {
        if(tag        == null ||
           tag.Length != 20)
            return null;

        var phTag = new ProfileTag();

        var tmp = new byte[4];

        phTag.Version   =  BigEndianBitConverter.ToUInt16(tag, 0);
        phTag.Kind      =  (byte)((tag[2] & 0xC0) >> 6);
        phTag.Reserved  =  (byte)(tag[2] & 0x3F);
        phTag.Volume    =  tag[3];
        phTag.FileId    =  BigEndianBitConverter.ToInt16(tag, 4);
        phTag.ValidChk  |= (tag[6]                                         & 0x80) == 0x80;
        phTag.UsedBytes =  (ushort)(BigEndianBitConverter.ToUInt16(tag, 6) & 0x7FFF);

        tmp[0]        = 0x00;
        tmp[1]        = tag[8];
        tmp[2]        = tag[9];
        tmp[3]        = tag[10];
        phTag.AbsPage = BigEndianBitConverter.ToUInt32(tmp, 0);

        phTag.Checksum = tag[11];
        phTag.RelPage  = BigEndianBitConverter.ToUInt16(tag, 12);

        tmp[0]          = 0x00;
        tmp[1]          = tag[14];
        tmp[2]          = tag[15];
        tmp[3]          = tag[16];
        phTag.NextBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

        tmp[0]          = 0x00;
        tmp[1]          = tag[17];
        tmp[2]          = tag[18];
        tmp[3]          = tag[19];
        phTag.PrevBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

        phTag.IsLast  = phTag.NextBlock == 0xFFFFFF;
        phTag.IsFirst = phTag.PrevBlock == 0xFFFFFF;

        return phTag;
    }

    /// <summary>Decodes tag from a Priam</summary>
    /// <param name="tag">Byte array containing raw tag data</param>
    /// <returns>Decoded tag in Priam's format</returns>
    public static PriamTag? DecodePriamTag(byte[] tag)
    {
        if(tag        == null ||
           tag.Length != 24)
            return null;

        var pmTag = new PriamTag();

        var tmp = new byte[4];

        pmTag.Version   =  BigEndianBitConverter.ToUInt16(tag, 0);
        pmTag.Kind      =  (byte)((tag[2] & 0xC0) >> 6);
        pmTag.Reserved  =  (byte)(tag[2] & 0x3F);
        pmTag.Volume    =  tag[3];
        pmTag.FileId    =  BigEndianBitConverter.ToInt16(tag, 4);
        pmTag.ValidChk  |= (tag[6]                                         & 0x80) == 0x80;
        pmTag.UsedBytes =  (ushort)(BigEndianBitConverter.ToUInt16(tag, 6) & 0x7FFF);

        tmp[0]        = 0x00;
        tmp[1]        = tag[8];
        tmp[2]        = tag[9];
        tmp[3]        = tag[10];
        pmTag.AbsPage = BigEndianBitConverter.ToUInt32(tmp, 0);

        pmTag.Checksum = tag[11];
        pmTag.RelPage  = BigEndianBitConverter.ToUInt16(tag, 12);

        tmp[0]          = 0x00;
        tmp[1]          = tag[14];
        tmp[2]          = tag[15];
        tmp[3]          = tag[16];
        pmTag.NextBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

        tmp[0]          = 0x00;
        tmp[1]          = tag[17];
        tmp[2]          = tag[18];
        tmp[3]          = tag[19];
        pmTag.PrevBlock = BigEndianBitConverter.ToUInt32(tmp, 0);

        pmTag.DiskSize = BigEndianBitConverter.ToUInt32(tag, 20);

        pmTag.IsLast  = pmTag.NextBlock == 0xFFFFFF;
        pmTag.IsFirst = pmTag.PrevBlock == 0xFFFFFF;

        return pmTag;
    }

    /// <summary>Decodes tag from any known format</summary>
    /// <param name="tag">Byte array containing raw tag data</param>
    /// <returns>Decoded tag in Priam's format</returns>
    public static PriamTag? DecodeTag(byte[] tag)
    {
        if(tag == null)
            return null;

        PriamTag pmTag;

        switch(tag.Length)
        {
            case 12:
                SonyTag? snTag = DecodeSonyTag(tag);

                if(snTag == null)
                    return null;

                pmTag = new PriamTag
                {
                    AbsPage   = 0,
                    Checksum  = 0,
                    DiskSize  = 0,
                    FileId    = snTag.Value.FileId,
                    Kind      = snTag.Value.Kind,
                    NextBlock = snTag.Value.NextBlock,
                    PrevBlock = snTag.Value.PrevBlock,
                    RelPage   = snTag.Value.RelPage,
                    Reserved  = snTag.Value.Reserved,
                    UsedBytes = 0,
                    ValidChk  = false,
                    Version   = snTag.Value.Version,
                    Volume    = snTag.Value.Volume,
                    IsFirst   = snTag.Value.IsFirst,
                    IsLast    = snTag.Value.IsLast
                };

                return pmTag;
            case 20:
                ProfileTag? phTag = DecodeProfileTag(tag);

                if(phTag == null)
                    return null;

                pmTag = new PriamTag
                {
                    AbsPage   = phTag.Value.AbsPage,
                    Checksum  = phTag.Value.Checksum,
                    DiskSize  = 0,
                    FileId    = phTag.Value.FileId,
                    Kind      = phTag.Value.Kind,
                    NextBlock = phTag.Value.NextBlock,
                    PrevBlock = phTag.Value.PrevBlock,
                    RelPage   = phTag.Value.RelPage,
                    Reserved  = phTag.Value.Reserved,
                    UsedBytes = phTag.Value.UsedBytes,
                    ValidChk  = phTag.Value.ValidChk,
                    Version   = phTag.Value.Version,
                    Volume    = phTag.Value.Volume,
                    IsFirst   = phTag.Value.IsFirst,
                    IsLast    = phTag.Value.IsLast
                };

                return pmTag;
            case 24: return DecodePriamTag(tag);
            default: return null;
        }
    }

    /// <summary>LisaOS tag as stored on Apple Profile and FileWare disks (20 bytes)</summary>
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
        /// <summary>0x06 bit 7, checksum valid?</summary>
        public bool ValidChk;
        /// <summary>0x06 bits 6 to 0, used bytes in block</summary>
        public ushort UsedBytes;
        /// <summary>0x08, 3 bytes, absolute page number</summary>
        public uint AbsPage;
        /// <summary>0x0B, checksum of data</summary>
        public byte Checksum;
        /// <summary>0x0C, relative page number</summary>
        public ushort RelPage;
        /// <summary>0x0E, 3 bytes, next block, 0xFFFFFF if it's last block</summary>
        public uint NextBlock;
        /// <summary>0x11, 3 bytes, previous block, 0xFFFFFF if it's first block</summary>
        public uint PrevBlock;

        /// <summary>On-memory value for easy first block search.</summary>
        public bool IsFirst;
        /// <summary>On-memory value for easy last block search.</summary>
        public bool IsLast;

        /// <summary>Converts this tag to Priam DataTower format</summary>
        public PriamTag ToPriam() => new()
        {
            AbsPage   = AbsPage,
            Checksum  = Checksum,
            FileId    = FileId,
            IsFirst   = IsFirst,
            IsLast    = IsLast,
            Kind      = Kind,
            NextBlock = IsLast ? 0xFFFFFF : NextBlock  & 0xFFFFFF,
            PrevBlock = IsFirst ? 0xFFFFFF : PrevBlock & 0xFFFFFF,
            RelPage   = RelPage,
            UsedBytes = UsedBytes,
            ValidChk  = ValidChk,
            Version   = Version,
            Volume    = Volume
        };

        /// <summary>Converts this tag to Sony format</summary>
        public SonyTag ToSony() => new()
        {
            FileId    = FileId,
            IsFirst   = IsFirst,
            IsLast    = IsLast,
            Kind      = Kind,
            NextBlock = (ushort)NextBlock,
            PrevBlock = (ushort)PrevBlock,
            RelPage   = RelPage,
            Version   = Version,
            Volume    = Volume
        };

        /// <summary>Gets a byte array representation of this tag</summary>
        public byte[] GetBytes()
        {
            var tagBytes = new byte[20];

            byte[] tmp = BigEndianBitConverter.GetBytes(Version);
            Array.Copy(tmp, 0, tagBytes, 0, 2);
            tagBytes[2] = (byte)(Kind << 6);
            tagBytes[3] = Volume;
            tmp         = BigEndianBitConverter.GetBytes(FileId);
            Array.Copy(tmp, 0, tagBytes, 4, 2);
            tmp = BigEndianBitConverter.GetBytes((ushort)(UsedBytes & 0x7FFF));
            Array.Copy(tmp, 0, tagBytes, 6, 2);

            if(ValidChk)
                tagBytes[6] += 0x80;

            tmp = BigEndianBitConverter.GetBytes(AbsPage);
            Array.Copy(tmp, 1, tagBytes, 8, 3);
            tagBytes[11] = Checksum;
            tmp          = BigEndianBitConverter.GetBytes(RelPage);
            Array.Copy(tmp, 0, tagBytes, 12, 2);
            tmp = BigEndianBitConverter.GetBytes(IsLast ? 0xFFFFFF : NextBlock);
            Array.Copy(tmp, 1, tagBytes, 14, 3);
            tmp = BigEndianBitConverter.GetBytes(IsFirst ? 0xFFFFFF : PrevBlock);
            Array.Copy(tmp, 1, tagBytes, 17, 3);

            return tagBytes;
        }
    }

    /// <summary>LisaOS tag as stored on Priam DataTower disks (24 bytes)</summary>
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
        /// <summary>0x06 bit 7, checksum valid?</summary>
        public bool ValidChk;
        /// <summary>0x06 bits 6 to 0, used bytes in block</summary>
        public ushort UsedBytes;
        /// <summary>0x08, 3 bytes, absolute page number</summary>
        public uint AbsPage;
        /// <summary>0x0B, checksum of data</summary>
        public byte Checksum;
        /// <summary>0x0C, relative page number</summary>
        public ushort RelPage;
        /// <summary>0x0E, 3 bytes, next block, 0xFFFFFF if it's last block</summary>
        public uint NextBlock;
        /// <summary>0x11, 3 bytes, previous block, 0xFFFFFF if it's first block</summary>
        public uint PrevBlock;
        /// <summary>0x14, disk size</summary>
        public uint DiskSize;

        /// <summary>On-memory value for easy first block search.</summary>
        public bool IsFirst;
        /// <summary>On-memory value for easy last block search.</summary>
        public bool IsLast;

        /// <summary>Converts this tag to Apple Profile format</summary>
        public ProfileTag ToProfile() => new()
        {
            AbsPage   = AbsPage,
            Checksum  = Checksum,
            FileId    = FileId,
            IsFirst   = IsFirst,
            IsLast    = IsLast,
            Kind      = Kind,
            NextBlock = IsLast ? 0xFFFFFF : NextBlock  & 0xFFFFFF,
            PrevBlock = IsFirst ? 0xFFFFFF : PrevBlock & 0xFFFFFF,
            RelPage   = RelPage,
            UsedBytes = UsedBytes,
            ValidChk  = ValidChk,
            Version   = Version,
            Volume    = Volume
        };

        /// <summary>Converts this tag to Sony format</summary>
        public SonyTag ToSony() => new()
        {
            FileId    = FileId,
            IsFirst   = IsFirst,
            IsLast    = IsLast,
            Kind      = Kind,
            NextBlock = (ushort)(IsLast ? 0x7FF : NextBlock  & 0x7FF),
            PrevBlock = (ushort)(IsFirst ? 0x7FF : PrevBlock & 0x7FF),
            RelPage   = RelPage,
            Version   = Version,
            Volume    = Volume
        };

        /// <summary>Gets a byte array representation of this tag</summary>
        public byte[] GetBytes()
        {
            var tagBytes = new byte[24];

            byte[] tmp = BigEndianBitConverter.GetBytes(Version);
            Array.Copy(tmp, 0, tagBytes, 0, 2);
            tagBytes[2] = (byte)(Kind << 6);
            tagBytes[3] = Volume;
            tmp         = BigEndianBitConverter.GetBytes(FileId);
            Array.Copy(tmp, 0, tagBytes, 4, 2);
            tmp = BigEndianBitConverter.GetBytes((ushort)(UsedBytes & 0x7FFF));
            Array.Copy(tmp, 0, tagBytes, 6, 2);

            if(ValidChk)
                tagBytes[6] += 0x80;

            tmp = BigEndianBitConverter.GetBytes(AbsPage);
            Array.Copy(tmp, 1, tagBytes, 8, 3);
            tagBytes[11] = Checksum;
            tmp          = BigEndianBitConverter.GetBytes(RelPage);
            Array.Copy(tmp, 0, tagBytes, 12, 2);
            tmp = BigEndianBitConverter.GetBytes(IsLast ? 0xFFFFFF : NextBlock);
            Array.Copy(tmp, 1, tagBytes, 14, 3);
            tmp = BigEndianBitConverter.GetBytes(IsFirst ? 0xFFFFFF : PrevBlock);
            Array.Copy(tmp, 1, tagBytes, 17, 3);
            tmp = BigEndianBitConverter.GetBytes(DiskSize);
            Array.Copy(tmp, 0, tagBytes, 20, 4);

            return tagBytes;
        }
    }

    /// <summary>LisaOS tag as stored on Apple Sony disks (12 bytes)</summary>
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
        /// <summary>0x06, relative page number</summary>
        public ushort RelPage;
        /// <summary>0x08, 3 bytes, next block, 0x7FF if it's last block, 0x8000 set if block is valid</summary>
        public ushort NextBlock;
        /// <summary>0x0A, 3 bytes, previous block, 0x7FF if it's first block</summary>
        public ushort PrevBlock;

        /// <summary>On-memory value for easy first block search.</summary>
        public bool IsFirst;
        /// <summary>On-memory value for easy last block search.</summary>
        public bool IsLast;

        /// <summary>Converts this tag to Apple Profile format</summary>
        public ProfileTag ToProfile() => new()
        {
            FileId    = FileId,
            IsFirst   = IsFirst,
            IsLast    = IsLast,
            Kind      = Kind,
            NextBlock = (uint)(IsLast ? 0xFFFFFF : NextBlock  & 0xFFFFFF),
            PrevBlock = (uint)(IsFirst ? 0xFFFFFF : PrevBlock & 0xFFFFFF),
            RelPage   = RelPage,
            Version   = Version,
            Volume    = Volume
        };

        /// <summary>Converts this tag to Priam DataTower format</summary>
        public PriamTag ToPriam() => new()
        {
            FileId    = FileId,
            IsFirst   = IsFirst,
            IsLast    = IsLast,
            Kind      = Kind,
            NextBlock = (uint)(IsLast ? 0xFFFFFF : NextBlock  & 0xFFFFFF),
            PrevBlock = (uint)(IsFirst ? 0xFFFFFF : PrevBlock & 0xFFFFFF),
            RelPage   = RelPage,
            Version   = Version,
            Volume    = Volume
        };

        /// <summary>Gets a byte array representation of this tag</summary>
        public byte[] GetBytes()
        {
            var tagBytes = new byte[12];

            byte[] tmp = BigEndianBitConverter.GetBytes(Version);
            Array.Copy(tmp, 0, tagBytes, 0, 2);
            tagBytes[2] = (byte)(Kind << 6);
            tagBytes[3] = Volume;
            tmp         = BigEndianBitConverter.GetBytes(FileId);
            Array.Copy(tmp, 0, tagBytes, 4, 2);
            tmp = BigEndianBitConverter.GetBytes(RelPage);
            Array.Copy(tmp, 0, tagBytes, 6, 2);
            tmp = BigEndianBitConverter.GetBytes(IsLast ? 0x7FF : NextBlock);
            Array.Copy(tmp, 1, tagBytes, 8, 2);
            tmp = BigEndianBitConverter.GetBytes(IsFirst ? 0x7FF : PrevBlock);
            Array.Copy(tmp, 1, tagBytes, 10, 2);

            return tagBytes;
        }
    }
}