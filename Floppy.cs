/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : BD.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Decoders.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Decodes DVD structures.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Runtime.InteropServices;

namespace DiscImageChef.Decoders
{
    // Information from:
    // National Semiconductor PC87332VLJ datasheet
    // SMsC FDC37C78 datasheet
    // Intel 82078 datasheet
    // Intel 82077AA datasheet
    // Toshiba TC8566AF datasheet
    // Fujitsu MB8876A datasheet
    // Inside Macintosh, Volume II, ISBN 0-201-17732-3
    // ECMA-147
    // ECMA-100
    public static class Floppy
    {
        #region Public enumerations

        /// <summary>
        /// In-sector code for sector size
        /// </summary>
        public enum IBMSectorSizeCode : byte
        {
            /// <summary>
            /// 128 bytes/sector
            /// </summary>
            EighthKilo = 0,
            /// <summary>
            /// 256 bytes/sector
            /// </summary>
            QuarterKilo = 1,
            /// <summary>
            /// 512 bytes/sector
            /// </summary>
            HalfKilo = 2,
            /// <summary>
            /// 1024 bytes/sector
            /// </summary>
            Kilo = 3,
            /// <summary>
            /// 2048 bytes/sector
            /// </summary>
            TwiceKilo = 4,
            /// <summary>
            /// 4096 bytes/sector
            /// </summary>
            FriceKilo = 5,
            /// <summary>
            /// 8192 bytes/sector
            /// </summary>
            TwiceFriceKilo = 6,
            /// <summary>
            /// 16384 bytes/sector
            /// </summary>
            FricelyFriceKilo = 7
        }

        public enum IBMIdType : byte
        {
            IndexMark = 0xFC,
            AddressMark = 0xFE,
            DataMark = 0xFB,
            DeletedDataMark = 0xF8
        }

        public enum AppleEncodedFormat : byte
        {
            /// <summary>
            /// Disk is an Apple II 3.5" disk
            /// </summary>
            AppleII = 0x96,
            /// <summary>
            /// Disk is an Apple Lisa 3.5" disk
            /// </summary>
            Lisa = 0x97,
            /// <summary>
            /// Disk is an Apple Macintosh single-sided 3.5" disk
            /// </summary>
            MacSingleSide = 0x9A,
            /// <summary>
            /// Disk is an Apple Macintosh double-sided 3.5" disk
            /// </summary>
            MacDoubleSide = 0xD9
        }

        #endregion Public enumerations

        #region Public structures

        #region IBM System 3740 floppy

        /// <summary>
        /// Track format for IBM System 3740 floppy
        /// </summary>
        public struct IBMFMTrack
        {
            /// <summary>
            /// Start of track
            /// </summary>
            public IBMFMTrackPreamble trackStart;
            /// <summary>
            /// Track sectors
            /// </summary>
            public IBMFMSector[] sectors;
            /// <summary>
            /// Undefined size
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// Start of IBM PC FM floppy track
        /// </summary>
        public struct IBMFMTrackPreamble
        {
            /// <summary>
            /// Gap from index pulse, 80 bytes set to 0xFF
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] gap;
            /// <summary>
            /// 6 bytes set to 0x00
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] zero;
            /// <summary>
            /// Set to <see cref="IBMIdType.IndexMark"/> 
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// Gap until first sector, 26 bytes to 0xFF
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public byte[] gap1;
        }

        /// <summary>
        /// Raw demodulated format for IBM System 3740 floppies
        /// </summary>
        public struct IBMFMSector
        {
            /// <summary>
            /// Sector address mark
            /// </summary>
            public IBMFMSectorAddressMark addressMark;
            /// <summary>
            /// 11 bytes set to 0xFF
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] innerGap;
            /// <summary>
            /// Sector data block
            /// </summary>
            public IBMFMSectorAddressMark dataBlock;
            /// <summary>
            /// Variable bytes set to 0xFF
            /// </summary>
            public byte[] outerGap;
        }

        /// <summary>
        /// Sector address mark for IBM System 3740 floppies, contains sync word
        /// </summary>
        public struct IBMFMSectorAddressMark
        {
            /// <summary>
            /// 6 bytes set to 0
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] zero;
            /// <summary>
            /// Set to <see cref="IBMIdType.AddressMark"/>
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// Track number
            /// </summary>
            public byte track;
            /// <summary>
            /// Side number
            /// </summary>
            public byte side;
            /// <summary>
            /// Sector number
            /// </summary>
            public byte sector;
            /// <summary>
            /// <see cref="IBMSectorSizeCode"/> 
            /// </summary>
            public IBMSectorSizeCode sectorSize;
            /// <summary>
            /// CRC16 from <see cref="type"/> to end of <see cref="sectorSize"/> 
            /// </summary>
            public UInt16 crc;
        }

        /// <summary>
        /// Sector data block for IBM System 3740 floppies
        /// </summary>
        public struct IBMFMSectorDataBlock
        {
            /// <summary>
            /// 12 bytes set to 0
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] zero;
            /// <summary>
            /// Set to <see cref="IBMIdType.DataMark"/> or to <see cref="IBMIdType.DeletedDataMark"/>
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// User data
            /// </summary>
            public byte[] data;
            /// <summary>
            /// CRC16 from <see cref="type"/> to end of <see cref="data"/> 
            /// </summary>
            public UInt16 crc;
        }

        #endregion IBM System 3740 floppy

        #region IBM System 34 floppy

        /// <summary>
        /// Track format for IBM System 34 floppy
        /// Used by IBM PC, Apple Macintosh (high-density only), and a lot others
        /// </summary>
        public struct IBMMFMTrack
        {
            /// <summary>
            /// Start of track
            /// </summary>
            public IBMMFMTrackPreamble trackStart;
            /// <summary>
            /// Track sectors
            /// </summary>
            public IBMMFMSector[] sectors;
            /// <summary>
            /// Undefined size
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// Start of IBM PC MFM floppy track
        /// Used by IBM PC, Apple Macintosh (high-density only), and a lot others
        /// </summary>
        public struct IBMMFMTrackPreamble
        {
            /// <summary>
            /// Gap from index pulse, 80 bytes set to 0x4E
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] gap;
            /// <summary>
            /// 12 bytes set to 0x00
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] zero;
            /// <summary>
            /// 3 bytes set to 0xC2
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] ctwo;
            /// <summary>
            /// Set to <see cref="IBMIdType.IndexMark"/> 
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// Gap until first sector, 50 bytes to 0x4E
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public byte[] gap1;
        }

        /// <summary>
        /// Raw demodulated format for IBM System 34 floppies
        /// </summary>
        public struct IBMMFMSector
        {
            /// <summary>
            /// Sector address mark
            /// </summary>
            public IBMMFMSectorAddressMark addressMark;
            /// <summary>
            /// 22 bytes set to 0x4E, set to 0x22 on Commodore 1581
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
            public byte[] innerGap;
            /// <summary>
            /// Sector data block
            /// </summary>
            public IBMMFMSectorAddressMark dataBlock;
            /// <summary>
            /// Variable bytes set to 0x4E, ECMA defines 54
            /// </summary>
            public byte[] outerGap;
        }

        /// <summary>
        /// Sector address mark for IBM System 34 floppies, contains sync word
        /// </summary>
        public struct IBMMFMSectorAddressMark
        {
            /// <summary>
            /// 12 bytes set to 0
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] zero;
            /// <summary>
            /// 3 bytes set to 0xA1
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] aone;
            /// <summary>
            /// Set to <see cref="IBMIdType.AddressMark"/>
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// Track number
            /// </summary>
            public byte track;
            /// <summary>
            /// Side number
            /// </summary>
            public byte side;
            /// <summary>
            /// Sector number
            /// </summary>
            public byte sector;
            /// <summary>
            /// <see cref="IBMSectorSizeCode"/> 
            /// </summary>
            public IBMSectorSizeCode sectorSize;
            /// <summary>
            /// CRC16 from <see cref="IBMMFMSectorAddressMark.aone"/> to end of <see cref="sectorSize"/> 
            /// </summary>
            public UInt16 crc;
        }

        /// <summary>
        /// Sector data block for IBM System 34 floppies
        /// </summary>
        public struct IBMMFMSectorDataBlock
        {
            /// <summary>
            /// 12 bytes set to 0
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] zero;
            /// <summary>
            /// 3 bytes set to 0xA1
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] aone;
            /// <summary>
            /// Set to <see cref="IBMIdType.DataMark"/> or to <see cref="IBMIdType.DeletedDataMark"/>
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// User data
            /// </summary>
            public byte[] data;
            /// <summary>
            /// CRC16 from <see cref="aone"/> to end of <see cref="data"/> 
            /// </summary>
            public UInt16 crc;
        }

        #endregion IBM System 34 floppy

        #region Perpendicular MFM floppy

        /// <summary>
        /// Perpendicular floppy track
        /// </summary>
        public struct PerpendicularFloppyTrack
        {
            /// <summary>
            /// Start of track
            /// </summary>
            public IBMMFMTrackPreamble trackStart;
            /// <summary>
            /// Track sectors
            /// </summary>
            public PerpendicularFloppySector[] sectors;
            /// <summary>
            /// Undefined size
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// Raw demodulated format for perpendicular floppies
        /// </summary>
        public struct PerpendicularFloppySector
        {
            /// <summary>
            /// Sector address mark
            /// </summary>
            public IBMMFMSectorAddressMark addressMark;
            /// <summary>
            /// 41 bytes set to 0x4E
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 41)]
            public byte[] innerGap;
            /// <summary>
            /// Sector data block
            /// </summary>
            public IBMMFMSectorDataBlock dataBlock;
            /// <summary>
            /// Variable-sized inter-sector gap, ECMA defines 83 bytes
            /// </summary>
            public byte[] outerGap;
        }

        #endregion Perpendicular MFM floppy

        #region ISO floppy

        /// <summary>
        /// ISO floppy track, also used by Atari ST and others
        /// </summary>
        public struct ISOFloppyTrack
        {
            /// <summary>
            /// Start of track, 32 bytes set to 0x4E
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] innerGap;
            /// <summary>
            /// Track sectors
            /// </summary>
            public IBMMFMSector[] sectors;
            /// <summary>
            /// Undefined size
            /// </summary>
            public byte[] gap;
        }

        #endregion ISO floppy

        #region Apple ][ GCR floppy

        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy track
        /// </summary>
        public struct AppleOldGCRRawSectorRawTrack
        {
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, between 40 and 95 bytes
            /// </summary>
            public byte[] gap;
            public AppleOldGCRRawSector[] sectors;
        }

        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy sector
        /// </summary>
        public struct AppleOldGCRRawSector
        {
            /// <summary>
            /// Address field
            /// </summary>
            public AppleOldGCRRawAddressField addressField;
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, between 5 and 10 bytes
            /// </summary>
            public byte[] innerGap;
            /// <summary>
            /// Data field
            /// </summary>
            public AppleOldGCRRawDataField dataField;
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, between 14 and 24 bytes
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy sector address field
        /// </summary>
        public struct AppleOldGCRRawAddressField
        {
            /// <summary>
            /// Always 0xD5, 0xAA, 0x96
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] prologue;
            /// <summary>
            /// Volume number encoded as:
            /// volume[0] = (decodedVolume >> 1) | 0xAA
            /// volume[1] = decodedVolume | 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] volume;
            /// <summary>
            /// Track number encoded as:
            /// track[0] = (decodedTrack >> 1) | 0xAA
            /// track[1] = decodedTrack | 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] track;
            /// <summary>
            /// Sector number encoded as:
            /// sector[0] = (decodedSector >> 1) | 0xAA
            /// sector[1] = decodedSector | 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] sector;
            /// <summary>
            /// decodedChecksum = decodedVolume ^ decodedTrack ^ decodedSector
            /// checksum[0] = (decodedChecksum >> 1) | 0xAA
            /// checksum[1] = decodedChecksum | 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] checksum;
            /// <summary>
            /// Always 0xDE, 0xAA, 0xEB
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] epilogue;
        }

        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy sector data field
        /// </summary>
        public struct AppleOldGCRRawDataField
        {
            /// <summary>
            /// Always 0xD5, 0xAA, 0xAD
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] prologue;
            /// <summary>
            /// Encoded data bytes.
            /// 410 bytes for 5to3 (aka DOS 3.2) format
            /// 342 bytes for 6to2 (aka DOS 3.3) format
            /// </summary>
            public byte[] data;
            public byte checksum;
            /// <summary>
            /// Always 0xDE, 0xAA, 0xEB
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] epilogue;
        }

        #endregion Apple ][ GCR floppy

        #region Apple Sony GCR floppy

        /// <summary>
        /// GCR-encoded Apple Sony GCR floppy track
        /// </summary>
        public struct AppleSonyGCRRawSectorRawTrack
        {
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, 36 bytes
            /// </summary>
            public byte[] gap;
            public AppleOldGCRRawSector[] sectors;
        }

        /// <summary>
        /// GCR-encoded Apple Sony GCR floppy sector
        /// </summary>
        public struct AppleSonyGCRRawSector
        {
            /// <summary>
            /// Address field
            /// </summary>
            public AppleSonyGCRRawAddressField addressField;
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, 6 bytes
            /// </summary>
            public byte[] innerGap;
            /// <summary>
            /// Data field
            /// </summary>
            public AppleSonyGCRRawDataField dataField;
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, unknown size
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// GCR-encoded Apple Sony GCR floppy sector address field
        /// </summary>
        public struct AppleSonyGCRRawAddressField
        {
            /// <summary>
            /// Always 0xD5, 0xAA, 0x96
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] prologue;
            /// <summary>
            /// Encoded (decodedTrack & 0x3F)
            /// </summary>
            public byte track;
            /// <summary>
            /// Encoded sector number
            /// </summary>
            public byte sector;
            /// <summary>
            /// Encoded side number
            /// </summary>
            public byte side;
            /// <summary>
            /// Disk format
            /// </summary>
            public AppleEncodedFormat format;
            /// <summary>
            /// Checksum
            /// </summary>
            public byte checksum;
            /// <summary>
            /// Always 0xDE, 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] epilogue;
        }

        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy sector data field
        /// </summary>
        public struct AppleSonyGCRRawDataField
        {
            /// <summary>
            /// Always 0xD5, 0xAA, 0xAD
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] prologue;
            /// <summary>
            /// Spare, usually <see cref="AppleSonyGCRRawAddressField.sector"/> 
            /// </summary>
            public byte spare;
            /// <summary>
            /// Encoded data bytes.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 698)]
            public byte[] data;
            /// <summary>
            /// Checksum
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] checksum;
            /// <summary>
            /// Always 0xDE, 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] epilogue;
        }

        #endregion Apple Sony GCR floppy

        #region Commodore GCR decoded

        /// <summary>
        /// Decoded Commodore GCR sector header
        /// </summary>
        public struct CommodoreSectorHeader
        {
            /// <summary>
            /// Always 0x08
            /// </summary>
            public byte id;
            /// <summary>
            /// XOR of following fields
            /// </summary>
            public byte checksum;
            /// <summary>
            /// Sector number
            /// </summary>
            public byte sector;
            /// <summary>
            /// Track number
            /// </summary>
            public byte track;
            /// <summary>
            /// Format ID, unknown meaning
            /// </summary>
            public UInt16 format;
            /// <summary>
            /// Filled with 0x0F
            /// </summary>
            public UInt16 fill;
        }

        /// <summary>
        /// Decoded Commodore GCR sector data
        /// </summary>
        public struct CommodoreSectorData
        {
            /// <summary>
            /// Always 0x07
            /// </summary>
            public byte id;
            /// <summary>
            /// User data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte data;
            /// <summary>
            /// XOR of <see cref="data"/>
            /// </summary>
            public byte checksum;
            /// <summary>
            /// Filled with 0x0F
            /// </summary>
            public UInt16 fill;
        }

        #endregion Commodore GCR decoded

        #region Commodore Amiga

        public struct CommodoreAmigaSector
        {
            /// <summary>
            /// Set to 0x00
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] zero;
            /// <summary>
            /// Set to 0xA1
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] sync;
            /// <summary>
            /// Set to 0xFF
            /// </summary>
            public byte amiga;
            /// <summary>
            /// Track number
            /// </summary>
            public byte track;
            /// <summary>
            /// Sector number
            /// </summary>
            public byte sector;
            /// <summary>
            /// Remaining sectors til end of writing
            /// </summary>
            public byte remaining;
            /// <summary>
            /// OS dependent tag
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] label;
            /// <summary>
            /// Checksum from <see cref="amiga"/> to <see cref="label"/> 
            /// </summary>
            public UInt32 headerChecksum;
            /// <summary>
            /// Checksum from <see cref="data"/>
            /// </summary>
            public UInt32 dataChecksum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] data;
        }

        #endregion Commodore Amiga

        #endregion Public structures
    }
}

