// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CopyQM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Sydex CopyQM disk images.
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class CopyQm : ImagePlugin
    {
        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CopyQmHeader
        {
            /// <summary>0x00 magic, "CQ"</summary>
            public ushort magic;
            /// <summary>0x02 always 0x14</summary>
            public byte mark;
            /// <summary>0x03 Bytes per sector (part of FAT's BPB)</summary>
            public ushort sectorSize;
            /// <summary>0x05 Sectors per cluster (part of FAT's BPB)</summary>
            public byte sectorPerCluster;
            /// <summary>0x06 Reserved sectors (part of FAT's BPB)</summary>
            public ushort reservedSectors;
            /// <summary>0x08 Number of FAT copies (part of FAT's BPB)</summary>
            public byte fatCopy;
            /// <summary>0x09 Maximum number of entries in root directory (part of FAT's BPB)</summary>
            public ushort rootEntries;
            /// <summary>0x0B Sectors on disk (part of FAT's BPB)</summary>
            public ushort sectors;
            /// <summary>0x0D Media descriptor (part of FAT's BPB)</summary>
            public byte mediaType;
            /// <summary>0x0E Sectors per FAT (part of FAT's BPB)</summary>
            public ushort sectorsPerFat;
            /// <summary>0x10 Sectors per track (part of FAT's BPB)</summary>
            public ushort sectorsPerTrack;
            /// <summary>0x12 Heads (part of FAT's BPB)</summary>
            public ushort heads;
            /// <summary>0x14 Hidden sectors (part of FAT's BPB)</summary>
            public uint hidden;
            /// <summary>0x18 Sectors on disk (part of FAT's BPB)</summary>
            public uint sectorsBig;
            /// <summary>0x1C Description</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 60)] public string description;
            /// <summary>0x58 Blind mode. 0 = DOS, 1 = blind, 2 = HFS</summary>
            public byte blind;
            /// <summary>0x59 Density. 0 = Double, 1 = High, 2 = Quad/Extra</summary>
            public byte density;
            /// <summary>0x5A Cylinders in image</summary>
            public byte imageCylinders;
            /// <summary>0x5B Cylinders on disk</summary>
            public byte totalCylinders;
            /// <summary>0x5C CRC32 of data</summary>
            public uint crc;
            /// <summary>0x60 DOS volume label</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)] public string volumeLabel;
            /// <summary>0x6B Modification time</summary>
            public ushort time;
            /// <summary>0x6D Modification date</summary>
            public ushort date;
            /// <summary>0x6F Comment length</summary>
            public ushort commentLength;
            /// <summary>0x71 Sector base (first sector - 1)</summary>
            public byte secbs;
            /// <summary>0x72 Unknown</summary>
            public ushort unknown;
            /// <summary>0x74 Interleave</summary>
            public byte interleave;
            /// <summary>0x75 Skew</summary>
            public byte skew;
            /// <summary>0x76 Source drive type. 1 = 5.25" DD, 2 = 5.25" HD, 3 = 3.5" DD, 4 = 3.5" HD, 6 = 3.5" ED</summary>
            public byte drive;
            /// <summary>0x77 Filling bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)] public byte[] fill;
            /// <summary>0x84 Header checksum</summary>
            public byte headerChecksum;
        }
        #endregion Internal Structures

        #region Internal Constants
        const ushort COPYQM_MAGIC = 0x5143;
        const byte COPYQM_MARK = 0x14;

        const byte COPYQM_FAT = 0;
        const byte COPYQM_BLIND = 1;
        const byte COPYQM_HFS = 2;

        const byte COPYQM_525_DD = 1;
        const byte COPYQM_525_HD = 2;
        const byte COPYQM_35_DD = 3;
        const byte COPYQM_35_HD = 4;
        const byte COPYQM_35_ED = 6;

        readonly uint[] copyQmCrcTable =
        {
            0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3, 0x0EDB8832,
            0x79DCB8A4, 0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91, 0x1DB71064, 0x6AB020F2,
            0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856, 0x646BA8C0, 0xFD62F97A,
            0x8A65C9EC, 0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
            0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3,
            0x45DF5C75, 0xDCD60DCF, 0xABD13D59, 0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423,
            0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87, 0x58684C11, 0xC1611DAB,
            0xB6662D3D, 0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
            0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01, 0x6B6B51F4,
            0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950,
            0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65, 0x4DB26158, 0x3AB551CE, 0xA3BC0074,
            0xD4BB30E2, 0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0,
            0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086, 0x5768B525,
            0x206F85B3, 0xB966D409, 0xCE61E49F, 0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81,
            0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615,
            0x73DC1683, 0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
            0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7, 0xFED41B76,
            0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 0xD6D6A3E8, 0xA1D1937E,
            0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6,
            0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
            0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7,
            0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D, 0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F,
            0x72076785, 0x05005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7,
            0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
            0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45, 0xA00AE278,
            0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC,
            0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9, 0xBDBDF21C, 0xCABAC28A, 0x53B39330,
            0x24B4A3A6, 0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94,
            0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
        };
        #endregion Internal Constants

        #region Internal variables
        CopyQmHeader header;
        byte[] decodedDisk;
        MemoryStream decodedImage;

        bool headerChecksumOk;
        uint calculatedDataCrc;
        #endregion Internal variables

        public CopyQm()
        {
            Name = "Sydex CopyQM";
            PluginUuid = new Guid("147E927D-3A92-4E0C-82CD-142F5A4FA76D");
            ImageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                ImageHasPartitions = false,
                ImageHasSessions = false,
                ImageVersion = null,
                ImageApplication = null,
                ImageApplicationVersion = null,
                ImageCreator = null,
                ImageComments = null,
                MediaManufacturer = null,
                MediaModel = null,
                MediaSerialNumber = null,
                MediaBarcode = null,
                MediaPartNumber = null,
                MediaSequence = 0,
                LastMediaSequence = 0,
                DriveManufacturer = null,
                DriveModel = null,
                DriveSerialNumber = null,
                DriveFirmwareRevision = null
            };
        }

        #region Public methods
        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 133) return false;

            byte[] hdr = new byte[133];
            stream.Read(hdr, 0, 133);

            ushort magic = BitConverter.ToUInt16(hdr, 0);

            if(magic != COPYQM_MAGIC || hdr[0x02] != COPYQM_MARK || 133 + hdr[0x6F] >= stream.Length) return false;

            return true;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdr = new byte[133];

            stream.Read(hdr, 0, 133);
            header = new CopyQmHeader();
            IntPtr hdrPtr = Marshal.AllocHGlobal(133);
            Marshal.Copy(hdr, 0, hdrPtr, 133);
            header = (CopyQmHeader)Marshal.PtrToStructure(hdrPtr, typeof(CopyQmHeader));
            Marshal.FreeHGlobal(hdrPtr);

            DicConsole.DebugWriteLine("CopyQM plugin", "header.magic = 0x{0:X4}", header.magic);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.mark = 0x{0:X2}", header.mark);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.sectorSize = {0}", header.sectorSize);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.sectorPerCluster = {0}", header.sectorPerCluster);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.reservedSectors = {0}", header.reservedSectors);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.fatCopy = {0}", header.fatCopy);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.rootEntries = {0}", header.rootEntries);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.sectors = {0}", header.sectors);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.mediaType = 0x{0:X2}", header.mediaType);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.sectorsPerFat = {0}", header.sectorsPerFat);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.sectorsPerTrack = {0}", header.sectorsPerTrack);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.heads = {0}", header.heads);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.hidden = {0}", header.hidden);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.sectorsBig = {0}", header.sectorsBig);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.description = {0}", header.description);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.blind = {0}", header.blind);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.density = {0}", header.density);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.imageCylinders = {0}", header.imageCylinders);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.totalCylinders = {0}", header.totalCylinders);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.crc = 0x{0:X8}", header.crc);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.volumeLabel = {0}", header.volumeLabel);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.time = 0x{0:X4}", header.time);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.date = 0x{0:X4}", header.date);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.commentLength = {0}", header.commentLength);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.secbs = {0}", header.secbs);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.unknown = 0x{0:X4}", header.unknown);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.interleave = {0}", header.interleave);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.skew = {0}", header.skew);
            DicConsole.DebugWriteLine("CopyQM plugin", "header.drive = {0}", header.drive);

            byte[] cmt = new byte[header.commentLength];
            stream.Read(cmt, 0, header.commentLength);
            ImageInfo.ImageComments = StringHandlers.CToString(cmt);
            decodedImage = new MemoryStream();

            calculatedDataCrc = 0;

            while(stream.Position + 2 < stream.Length)
            {
                byte[] runLengthBytes = new byte[2];
                if(stream.Read(runLengthBytes, 0, 2) != 2) break;

                short runLength = BitConverter.ToInt16(runLengthBytes, 0);

                if(runLength < 0)
                {
                    byte repeatedByte = (byte)stream.ReadByte();
                    byte[] repeatedArray = new byte[runLength * -1];
                    ArrayHelpers.ArrayFill(repeatedArray, repeatedByte);

                    for(int i = 0; i < runLength * -1; i++)
                    {
                        decodedImage.WriteByte(repeatedByte);
                        calculatedDataCrc = copyQmCrcTable[(repeatedByte ^ calculatedDataCrc) & 0x3F] ^
                                            (calculatedDataCrc >> 8);
                    }
                }
                else if(runLength > 0)
                {
                    byte[] nonRepeated = new byte[runLength];
                    stream.Read(nonRepeated, 0, runLength);
                    decodedImage.Write(nonRepeated, 0, runLength);

                    foreach(byte c in nonRepeated) calculatedDataCrc = copyQmCrcTable[(c ^ calculatedDataCrc) & 0x3F] ^
                                                                       (calculatedDataCrc >> 8);
                }
            }

            // In case there is omitted data
            long sectors = header.sectorsPerTrack * header.heads * header.totalCylinders;

            long fillingLen = sectors * header.sectorSize - decodedImage.Length;

            if(fillingLen > 0)
            {
                byte[] filling = new byte[fillingLen];
                ArrayHelpers.ArrayFill(filling, (byte)0xF6);
                decodedImage.Write(filling, 0, filling.Length);
            }

            /*
            FileStream debugStream = new FileStream("debug.img", FileMode.CreateNew, FileAccess.ReadWrite);
            debugStream.Write(decodedImage.ToArray(), 0, (int)decodedImage.Length);
            debugStream.Close();
            */

            int sum = 0;
            for(int i = 0; i < hdr.Length - 1; i++) sum += hdr[i];

            headerChecksumOk = ((-1 * sum) & 0xFF) == header.headerChecksum;

            DicConsole.DebugWriteLine("CopyQM plugin", "Calculated header checksum = 0x{0:X2}, {1}",
                                      (-1 * sum) & 0xFF, headerChecksumOk);
            DicConsole.DebugWriteLine("CopyQM plugin", "Calculated data CRC = 0x{0:X8}, {1}", calculatedDataCrc,
                                      calculatedDataCrc == header.crc);

            ImageInfo.ImageApplication = "CopyQM";
            ImageInfo.ImageCreationTime = DateHandlers.DosToDateTime(header.date, header.time);
            ImageInfo.ImageLastModificationTime = ImageInfo.ImageCreationTime;
            ImageInfo.ImageName = header.volumeLabel;
            ImageInfo.ImageSize = (ulong)(stream.Length - 133 - header.commentLength);
            ImageInfo.Sectors = (ulong)sectors;
            ImageInfo.SectorSize = header.sectorSize;

            switch(header.drive)
            {
                case COPYQM_525_HD:
                    if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 15 &&
                       header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_HD;
                    else if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 16 &&
                            header.sectorSize == 256) ImageInfo.MediaType = MediaType.ACORN_525_DS_DD;
                    else if(header.heads == 1 && header.totalCylinders == 80 && header.sectorsPerTrack == 16 &&
                            header.sectorSize == 256) ImageInfo.MediaType = MediaType.ACORN_525_SS_DD_80;
                    else if(header.heads == 1 && header.totalCylinders == 80 && header.sectorsPerTrack == 10 &&
                            header.sectorSize == 256) ImageInfo.MediaType = MediaType.ACORN_525_SS_SD_80;
                    else if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 8 &&
                            header.sectorSize == 1024) ImageInfo.MediaType = MediaType.NEC_525_HD;
                    else if(header.heads == 2 && header.totalCylinders == 77 && header.sectorsPerTrack == 8 &&
                            header.sectorSize == 1024) ImageInfo.MediaType = MediaType.SHARP_525;
                    else goto case COPYQM_525_DD;
                    break;
                case COPYQM_525_DD:
                    if(header.heads == 1 && header.totalCylinders == 40 && header.sectorsPerTrack == 8 &&
                       header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_SS_DD_8;
                    else if(header.heads == 1 && header.totalCylinders == 40 && header.sectorsPerTrack == 9 &&
                            header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_SS_DD_9;
                    else if(header.heads == 2 && header.totalCylinders == 40 && header.sectorsPerTrack == 8 &&
                            header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_DS_DD_8;
                    else if(header.heads == 2 && header.totalCylinders == 40 && header.sectorsPerTrack == 9 &&
                            header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
                    else if(header.heads == 1 && header.totalCylinders == 40 && header.sectorsPerTrack == 18 &&
                            header.sectorSize == 128) ImageInfo.MediaType = MediaType.ATARI_525_SD;
                    else if(header.heads == 1 && header.totalCylinders == 40 && header.sectorsPerTrack == 26 &&
                            header.sectorSize == 128) ImageInfo.MediaType = MediaType.ATARI_525_ED;
                    else if(header.heads == 1 && header.totalCylinders == 40 && header.sectorsPerTrack == 18 &&
                            header.sectorSize == 256) ImageInfo.MediaType = MediaType.ATARI_525_DD;
                    else ImageInfo.MediaType = MediaType.Unknown;
                    break;
                case COPYQM_35_ED:
                    if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 36 &&
                       header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_ED;
                    else goto case COPYQM_35_HD;
                    break;
                case COPYQM_35_HD:
                    if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 18 &&
                       header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_HD;
                    else if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 21 &&
                            header.sectorSize == 512) ImageInfo.MediaType = MediaType.DMF;
                    else if(header.heads == 2 && header.totalCylinders == 82 && header.sectorsPerTrack == 21 &&
                            header.sectorSize == 512) ImageInfo.MediaType = MediaType.DMF_82;
                    else if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 8 &&
                            header.sectorSize == 1024) ImageInfo.MediaType = MediaType.NEC_35_HD_8;
                    else if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 15 &&
                            header.sectorSize == 512) ImageInfo.MediaType = MediaType.NEC_35_HD_15;
                    else goto case COPYQM_35_DD;
                    break;
                case COPYQM_35_DD:
                    if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 9 &&
                       header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                    else if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 8 &&
                            header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_DS_DD_8;
                    else if(header.heads == 1 && header.totalCylinders == 80 && header.sectorsPerTrack == 9 &&
                            header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_SS_DD_9;
                    else if(header.heads == 1 && header.totalCylinders == 80 && header.sectorsPerTrack == 8 &&
                            header.sectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_SS_DD_8;
                    else if(header.heads == 2 && header.totalCylinders == 80 && header.sectorsPerTrack == 5 &&
                            header.sectorSize == 1024) ImageInfo.MediaType = MediaType.ACORN_35_DS_DD;
                    else if(header.heads == 2 && header.totalCylinders == 77 && header.sectorsPerTrack == 8 &&
                            header.sectorSize == 1024) ImageInfo.MediaType = MediaType.SHARP_35;
                    else if(header.heads == 1 && header.totalCylinders == 70 && header.sectorsPerTrack == 9 &&
                            header.sectorSize == 512) ImageInfo.MediaType = MediaType.Apricot_35;
                    else ImageInfo.MediaType = MediaType.Unknown;
                    break;
                default:
                    ImageInfo.MediaType = MediaType.Unknown;
                    break;
            }

            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            decodedDisk = decodedImage.ToArray();

            decodedImage.Close();

            DicConsole.VerboseWriteLine("CopyQM image contains a disk of type {0}", ImageInfo.MediaType);
            if(!string.IsNullOrEmpty(ImageInfo.ImageComments))
                DicConsole.VerboseWriteLine("CopyQM comments: {0}", ImageInfo.ImageComments);

            ImageInfo.Heads = header.heads;
            ImageInfo.Cylinders = header.imageCylinders;
            ImageInfo.SectorsPerTrack = header.sectorsPerTrack;

            return true;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifyMediaImage()
        {
            return calculatedDataCrc == header.crc && headerChecksumOk;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.ImageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.ImageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.Sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.SectorSize;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.SectorSize];

            Array.Copy(decodedDisk, (int)sectorAddress * ImageInfo.SectorSize, buffer, 0,
                       length * ImageInfo.SectorSize);

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "Sydex CopyQM";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.ImageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.ImageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.ImageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.ImageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.ImageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.MediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.MediaModel;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.MediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.MediaBarcode;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.MediaPartNumber;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.MediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.LastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.DriveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.DriveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.DriveSerialNumber;
        }
        #endregion Public methods

        #region Unsupported features
        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Partition> GetPartitions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetTracks()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Session> GetSessions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        #endregion Unsupported features
    }
}