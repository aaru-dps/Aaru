// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : TeleDisk.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Sydex TeleDisk disk images.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.ImagePlugins
{
    // Created following notes from Dave Dunfield
    // http://www.classiccmp.org/dunfield/img54306/td0notes.txt
    class TeleDisk : ImagePlugin
    {
        #region Internal Structures

        struct TD0Header
        {
            /// <summary>"TD" or "td" depending on compression</summary>
            public ushort signature;
            /// <summary>Sequence, but TeleDisk seems to complaing if != 0</summary>
            public byte sequence;
            /// <summary>Random, same byte for all disks in the same set</summary>
            public byte diskSet;
            /// <summary>TeleDisk version, major in high nibble, minor in low nibble</summary>
            public byte version;
            /// <summary>Data rate</summary>
            public byte dataRate;
            /// <summary>BIOS drive type</summary>
            public byte driveType;
            /// <summary>Stepping used</summary>
            public byte stepping;
            /// <summary>If set means image only allocates sectors marked in-use by FAT12</summary>
            public byte dosAllocation;
            /// <summary>Sides of disk</summary>
            public byte sides;
            /// <summary>CRC of all the previous</summary>
            public ushort crc;
        }

        struct TDCommentBlockHeader
        {
            /// <summary>CRC of comment block after crc field</summary>
            public ushort crc;
            /// <summary>Length of comment</summary>
            public ushort length;
            public byte year;
            public byte month;
            public byte day;
            public byte hour;
            public byte minute;
            public byte second;
        }

        struct TDTrackHeader
        {
            /// <summary>Sectors in the track, 0xFF if end of disk image (there is no spoon)</summary>
            public byte sectors;
            /// <summary>Cylinder the head was on</summary>
            public byte cylinder;
            /// <summary>Head/side used</summary>
            public byte head;
            /// <summary>Lower byte of CRC of previous fields</summary>
            public byte crc;
        }

        struct TDSectorHeader
        {
            /// <summary>Cylinder as stored on sector address mark</summary>
            public byte cylinder;
            /// <summary>Head as stored on sector address mark</summary>
            public byte head;
            /// <summary>Sector number as stored on sector address mark</summary>
            public byte sectorNumber;
            /// <summary>Sector size</summary>
            public byte sectorSize;
            /// <summary>Sector flags</summary>
            public byte flags;
            /// <summary>Lower byte of TeleDisk CRC of sector header, data header and data block</summary>
            public byte crc;
        }

        struct TDDataHeader
        {
            /// <summary>Size of all data (encoded) + next field (1)</summary>
            public ushort dataSize;
            /// <summary>Encoding used for data block</summary>
            public byte dataEncoding;
        }

        #endregion

        #region Internal Constants

        // "TD" as little endian uint.
        const ushort tdMagic = 0x4454;
        // "td" as little endian uint. Means whole file is compressed (aka Advanced Compression)
        const ushort tdAdvCompMagic = 0x6474;

        // DataRates
        const byte DataRate250kbps = 0x00;
        const byte DataRate300kbps = 0x01;
        const byte DataRate500kbps = 0x02;

        // TeleDisk drive types
        const byte DriveType525HD_DDDisk = 0x00;
        const byte DriveType525HD = 0x01;
        const byte DriveType525DD = 0x02;
        const byte DriveType35DD = 0x03;
        const byte DriveType35HD = 0x04;
        const byte DriveType8inch = 0x05;
        const byte DriveType35ED = 0x06;

        // Stepping
        const byte SteppingSingle = 0x00;
        const byte SteppingDouble = 0x01;
        const byte SteppingEvenOnly = 0x02;
        // If this bit is set, there is a comment block
        const byte CommentBlockPresent = 0x80;

        // CRC polynomial
        const ushort TeleDiskCRCPoly = 0xA097;

        // Sector sizes table
        const byte SectorSize128 = 0x00;
        const byte SectorSize256 = 0x01;
        const byte SectorSize512 = 0x02;
        const byte SectorSize1K = 0x03;
        const byte SectorSize2K = 0x04;
        const byte SectorSize4K = 0x05;
        const byte SectorSize8K = 0x06;

        // Flags
        // Address mark repeats inside same track
        const byte FlagsSectorDuplicate = 0x01;
        // Sector gave CRC error on reading
        const byte FlagsSectorCRCError = 0x02;
        // Address mark indicates deleted sector
        const byte FlagsSectorDeleted = 0x04;
        // Sector skipped as FAT said it's unused
        const byte FlagsSectorSkipped = 0x10;
        // There was an address mark, but no data following
        const byte FlagsSectorDataless = 0x20;
        // There was data without address mark
        const byte FlagsSectorNoID = 0x40;

        // Data block encodings
        // Data is copied as is
        const byte dataBlockCopy = 0x00;
        // Data is encoded as a pair of len.value uint16s
        const byte dataBlockPattern = 0x01;
        // Data is encoded as RLE
        const byte dataBlockRLE = 0x02;

        #endregion

        #region Internal variables

        TD0Header header;
        TDCommentBlockHeader commentHeader;
        byte[] commentBlock;
        Dictionary<uint, byte[]> sectorsData;
        // LBA, data
        uint totalDiskSize;
        bool ADiskCRCHasFailed;
        List<ulong> SectorsWhereCRCHasFailed;

        #endregion

        public TeleDisk()
        {
            Name = "Sydex TeleDisk";
            PluginUUID = new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageApplication = "Sydex TeleDisk";
            ImageInfo.imageComments = null;
            ImageInfo.imageCreator = null;
            ImageInfo.mediaManufacturer = null;
            ImageInfo.mediaModel = null;
            ImageInfo.mediaSerialNumber = null;
            ImageInfo.mediaBarcode = null;
            ImageInfo.mediaPartNumber = null;
            ImageInfo.mediaSequence = 0;
            ImageInfo.lastMediaSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
            ADiskCRCHasFailed = false;
            SectorsWhereCRCHasFailed = new List<ulong>();
        }

        public override bool IdentifyImage(string imagePath)
        {
            header = new TD0Header();
            byte[] headerBytes = new byte[12];
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);

            stream.Read(headerBytes, 0, 12);
            stream.Close();

            header.signature = BitConverter.ToUInt16(headerBytes, 0);

            if(header.signature != tdMagic && header.signature != tdAdvCompMagic)
                return false;

            header.sequence = headerBytes[2];
            header.diskSet = headerBytes[3];
            header.version = headerBytes[4];
            header.dataRate = headerBytes[5];
            header.driveType = headerBytes[6];
            header.stepping = headerBytes[7];
            header.dosAllocation = headerBytes[8];
            header.sides = headerBytes[9];
            header.crc = BitConverter.ToUInt16(headerBytes, 10);

            byte[] headerBytesForCRC = new byte[10];
            Array.Copy(headerBytes, headerBytesForCRC, 10);
            ushort calculatedHeaderCRC = TeleDiskCRC(0x0000, headerBytesForCRC);

            DicConsole.DebugWriteLine("TeleDisk plugin", "header.signature = 0x{0:X4}", header.signature);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.sequence = 0x{0:X2}", header.sequence);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.diskSet = 0x{0:X2}", header.diskSet);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.version = 0x{0:X2}", header.version);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.dataRate = 0x{0:X2}", header.dataRate);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.driveType = 0x{0:X2}", header.driveType);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.stepping = 0x{0:X2}", header.stepping);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.dosAllocation = 0x{0:X2}", header.dosAllocation);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.sides = 0x{0:X2}", header.sides);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.crc = 0x{0:X4}", header.crc);
            DicConsole.DebugWriteLine("TeleDisk plugin", "calculated header crc = 0x{0:X4}", calculatedHeaderCRC);

            // We need more checks as the magic is too simply.
            // This may deny legal images

            // That would be much of a coincidence
            if(header.crc == calculatedHeaderCRC)
                return true;

            if(header.sequence != 0x00)
                return false;

            if(header.dataRate != DataRate250kbps && header.dataRate != DataRate300kbps && header.dataRate != DataRate500kbps)
                return false;

            if(header.driveType != DriveType35DD && header.driveType != DriveType35ED && header.driveType != DriveType35HD && header.driveType != DriveType525DD &&
                header.driveType != DriveType525HD && header.driveType != DriveType525HD_DDDisk && header.driveType != DriveType8inch)
                return false;

            return true;
        }

        public override bool OpenImage(string imagePath)
        {
            header = new TD0Header();
            byte[] headerBytes = new byte[12];
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);

            stream.Read(headerBytes, 0, 12);

            header.signature = BitConverter.ToUInt16(headerBytes, 0);

            if(header.signature != tdMagic && header.signature != tdAdvCompMagic)
                return false;

            header.sequence = headerBytes[2];
            header.diskSet = headerBytes[3];
            header.version = headerBytes[4];
            header.dataRate = headerBytes[5];
            header.driveType = headerBytes[6];
            header.stepping = headerBytes[7];
            header.dosAllocation = headerBytes[8];
            header.sides = headerBytes[9];
            header.crc = BitConverter.ToUInt16(headerBytes, 10);

            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imagePath);
            ImageInfo.imageVersion = string.Format("{0}.{1}", (header.version & 0xF0) >> 4, header.version & 0x0F);
            ImageInfo.imageApplication = ImageInfo.imageVersion;

            byte[] headerBytesForCRC = new byte[10];
            Array.Copy(headerBytes, headerBytesForCRC, 10);
            ushort calculatedHeaderCRC = TeleDiskCRC(0x0000, headerBytesForCRC);

            DicConsole.DebugWriteLine("TeleDisk plugin", "header.signature = 0x{0:X4}", header.signature);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.sequence = 0x{0:X2}", header.sequence);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.diskSet = 0x{0:X2}", header.diskSet);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.version = 0x{0:X2}", header.version);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.dataRate = 0x{0:X2}", header.dataRate);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.driveType = 0x{0:X2}", header.driveType);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.stepping = 0x{0:X2}", header.stepping);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.dosAllocation = 0x{0:X2}", header.dosAllocation);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.sides = 0x{0:X2}", header.sides);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.crc = 0x{0:X4}", header.crc);
            DicConsole.DebugWriteLine("TeleDisk plugin", "calculated header crc = 0x{0:X4}", calculatedHeaderCRC);

            // We need more checks as the magic is too simply.
            // This may deny legal images

            // That would be much of a coincidence
            if(header.crc != calculatedHeaderCRC)
            {
                ADiskCRCHasFailed = true;
                DicConsole.DebugWriteLine("TeleDisk plugin", "Calculated CRC does not coincide with stored one.");
            }

            if(header.sequence != 0x00)
                return false;

            if(header.dataRate != DataRate250kbps && header.dataRate != DataRate300kbps && header.dataRate != DataRate500kbps)
                return false;

            if(header.driveType != DriveType35DD && header.driveType != DriveType35ED && header.driveType != DriveType35HD && header.driveType != DriveType525DD &&
                header.driveType != DriveType525HD && header.driveType != DriveType525HD_DDDisk && header.driveType != DriveType8inch)
                return false;

            if(header.signature == tdAdvCompMagic)
                throw new NotImplementedException("TeleDisk Advanced Compression support not yet implemented");

            ImageInfo.imageCreationTime = DateTime.MinValue;

            if((header.stepping & CommentBlockPresent) == CommentBlockPresent)
            {
                commentHeader = new TDCommentBlockHeader();

                byte[] commentHeaderBytes = new byte[10];
                byte[] commentBlockForCRC;

                stream.Read(commentHeaderBytes, 0, 10);
                commentHeader.crc = BitConverter.ToUInt16(commentHeaderBytes, 0);
                commentHeader.length = BitConverter.ToUInt16(commentHeaderBytes, 2);
                commentHeader.year = commentHeaderBytes[4];
                commentHeader.month = commentHeaderBytes[5];
                commentHeader.day = commentHeaderBytes[6];
                commentHeader.hour = commentHeaderBytes[7];
                commentHeader.minute = commentHeaderBytes[8];
                commentHeader.second = commentHeaderBytes[9];

                commentBlock = new byte[commentHeader.length];
                stream.Read(commentBlock, 0, commentHeader.length);

                commentBlockForCRC = new byte[commentHeader.length + 8];
                Array.Copy(commentHeaderBytes, 2, commentBlockForCRC, 0, 8);
                Array.Copy(commentBlock, 0, commentBlockForCRC, 8, commentHeader.length);

                ushort cmtcrc = TeleDiskCRC(0, commentBlockForCRC);

                DicConsole.DebugWriteLine("TeleDisk plugin", "Comment header");
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.crc = 0x{0:X4}", commentHeader.crc);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tCalculated CRC = 0x{0:X4}", cmtcrc);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.length = {0} bytes", commentHeader.length);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.year = {0}", commentHeader.year);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.month = {0}", commentHeader.month);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.day = {0}", commentHeader.day);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.hour = {0}", commentHeader.hour);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.minute = {0}", commentHeader.minute);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.second = {0}", commentHeader.second);

                ADiskCRCHasFailed |= cmtcrc != commentHeader.crc;

                for(int i = 0; i < commentBlock.Length; i++)
                {
                    // Replace NULLs, used by TeleDisk as newline markers, with UNIX newline marker
                    if(commentBlock[i] == 0x00)
                        commentBlock[i] = 0x0A;
                }

                ImageInfo.imageComments = System.Text.Encoding.ASCII.GetString(commentBlock);

                DicConsole.DebugWriteLine("TeleDisk plugin", "Comment");
                DicConsole.DebugWriteLine("TeleDisk plugin", "{0}", ImageInfo.imageComments);

                ImageInfo.imageCreationTime = new DateTime(commentHeader.year + 1900, commentHeader.month + 1, commentHeader.day,
                    commentHeader.hour, commentHeader.minute, commentHeader.second, DateTimeKind.Unspecified);
            }

            FileInfo fi = new FileInfo(imagePath);
            if(ImageInfo.imageCreationTime == DateTime.MinValue)
                ImageInfo.imageCreationTime = fi.CreationTimeUtc;
            ImageInfo.imageLastModificationTime = fi.LastWriteTimeUtc;

            DicConsole.DebugWriteLine("TeleDisk plugin", "Image created on {0}", ImageInfo.imageCreationTime);
            DicConsole.DebugWriteLine("TeleDisk plugin", "Image modified on {0}", ImageInfo.imageLastModificationTime);

            DicConsole.DebugWriteLine("TeleDisk plugin", "Parsing image");

            totalDiskSize = 0;
            byte spt = 0;
            ImageInfo.imageSize = 0;
            sectorsData = new Dictionary<uint, byte[]>();
            ImageInfo.sectorSize = 0;
            while(true)
            {
                TDTrackHeader TDTrack = new TDTrackHeader();
                byte[] TDTrackForCRC = new byte[3];
                byte TDTrackCalculatedCRC;

                TDTrack.sectors = (byte)stream.ReadByte();
                TDTrack.cylinder = (byte)stream.ReadByte();
                TDTrack.head = (byte)stream.ReadByte();
                TDTrack.crc = (byte)stream.ReadByte();

                TDTrackForCRC[0] = TDTrack.sectors;
                TDTrackForCRC[1] = TDTrack.cylinder;
                TDTrackForCRC[2] = TDTrack.head;

                TDTrackCalculatedCRC = (byte)(TeleDiskCRC(0, TDTrackForCRC) & 0xFF);

                DicConsole.DebugWriteLine("TeleDisk plugin", "Track follows");
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tTrack cylinder: {0}\t", TDTrack.cylinder);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tTrack head: {0}\t", TDTrack.head);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tSectors in track: {0}\t", TDTrack.sectors);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tTrack header CRC: 0x{0:X2} (calculated 0x{1:X2})\t", TDTrack.crc, TDTrackCalculatedCRC);

                ADiskCRCHasFailed |= TDTrackCalculatedCRC != TDTrack.crc;

                if(TDTrack.sectors == 0xFF) // End of disk image
                {
                    DicConsole.DebugWriteLine("TeleDisk plugin", "End of disk image arrived");
                    DicConsole.DebugWriteLine("TeleDisk plugin", "Total of {0} data sectors, for {1} bytes", sectorsData.Count, totalDiskSize);

                    break;
                }

                if(spt != TDTrack.sectors && TDTrack.sectors > 0)
                {
                    if(spt != 0)
                        throw new FeatureUnsupportedImageException("Variable number of sectors per track. This kind of image is not yet supported");
                    spt = TDTrack.sectors;
                }

                for(byte processedSectors = 0; processedSectors < TDTrack.sectors; processedSectors++)
                {
                    TDSectorHeader TDSector = new TDSectorHeader();
                    TDDataHeader TDData = new TDDataHeader();
                    byte[] dataSizeBytes = new byte[2];
                    byte[] data;
                    byte[] decodedData;

                    TDSector.cylinder = (byte)stream.ReadByte();
                    TDSector.head = (byte)stream.ReadByte();
                    TDSector.sectorNumber = (byte)stream.ReadByte();
                    TDSector.sectorSize = (byte)stream.ReadByte();
                    TDSector.flags = (byte)stream.ReadByte();
                    TDSector.crc = (byte)stream.ReadByte();

                    DicConsole.DebugWriteLine("TeleDisk plugin", "\tSector follows");
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tAddressMark cylinder: {0}", TDSector.cylinder);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tAddressMark head: {0}", TDSector.head);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tAddressMark sector number: {0}", TDSector.sectorNumber);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector size: {0}", TDSector.sectorSize);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector flags: 0x{0:X2}", TDSector.flags);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector CRC (plus headers): 0x{0:X2}", TDSector.crc);

                    uint LBA = (uint)((TDSector.cylinder * header.sides * spt) + (TDSector.head * spt) + (TDSector.sectorNumber - 1));
                    if((TDSector.flags & FlagsSectorDataless) != FlagsSectorDataless && (TDSector.flags & FlagsSectorSkipped) != FlagsSectorSkipped)
                    {
                        stream.Read(dataSizeBytes, 0, 2);
                        TDData.dataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                        TDData.dataSize--; // Sydex decided to including dataEncoding byte as part of it
                        ImageInfo.imageSize += TDData.dataSize;
                        TDData.dataEncoding = (byte)stream.ReadByte();
                        data = new byte[TDData.dataSize];
                        stream.Read(data, 0, TDData.dataSize);
                        DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tData size (in-image): {0}", TDData.dataSize);
                        DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tData encoding: 0x{0:X2}", TDData.dataEncoding);

                        decodedData = DecodeTeleDiskData(TDSector.sectorSize, TDData.dataEncoding, data);

                        byte TDSectorCalculatedCRC = (byte)(TeleDiskCRC(0, decodedData) & 0xFF);

                        if(TDSectorCalculatedCRC != TDSector.crc)
                        {
                            DicConsole.DebugWriteLine("TeleDisk plugin", "Sector LBA {0} calculated CRC 0x{1:X2} differs from stored CRC 0x{2:X2}", LBA, TDSectorCalculatedCRC, TDSector.crc);
                            if((TDSector.flags & FlagsSectorNoID) != FlagsSectorNoID)
                                if(!sectorsData.ContainsKey(LBA) && (TDSector.flags & FlagsSectorDuplicate) != FlagsSectorDuplicate)
                                    SectorsWhereCRCHasFailed.Add(LBA);
                        }
                    }
                    else
                    {
                        switch(TDSector.sectorSize)
                        {
                            case SectorSize128:
                                decodedData = new byte[128];
                                break;
                            case SectorSize256:
                                decodedData = new byte[256];
                                break;
                            case SectorSize512:
                                decodedData = new byte[512];
                                break;
                            case SectorSize1K:
                                decodedData = new byte[1024];
                                break;
                            case SectorSize2K:
                                decodedData = new byte[2048];
                                break;
                            case SectorSize4K:
                                decodedData = new byte[4096];
                                break;
                            case SectorSize8K:
                                decodedData = new byte[8192];
                                break;
                            default:
                                throw new ImageNotSupportedException(string.Format("Sector size {0} for cylinder {1} head {2} sector {3} is incorrect.",
                                    TDSector.sectorSize, TDSector.cylinder, TDSector.head, TDSector.sectorNumber));
                        }
                        ArrayHelpers.ArrayFill(decodedData, (byte)0);
                    }
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tLBA: {0}", LBA);

                    if((TDSector.flags & FlagsSectorNoID) != FlagsSectorNoID)
                    {
                        if(sectorsData.ContainsKey(LBA))
                        {
                            if((TDSector.flags & FlagsSectorDuplicate) == FlagsSectorDuplicate)
                            {
                                DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector {0} on cylinder {1} head {2} is duplicate, and marked so",
                                    TDSector.sectorNumber, TDSector.cylinder, TDSector.head);
                            }
                            else
                            {
                                DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector {0} on cylinder {1} head {2} is duplicate, but is not marked so",
                                    TDSector.sectorNumber, TDSector.cylinder, TDSector.head);
                            }
                        }
                        else
                        {
                            sectorsData.Add(LBA, decodedData);
                            totalDiskSize += (uint)decodedData.Length;
                        }
                    }
                    if(decodedData.Length > ImageInfo.sectorSize)
                        ImageInfo.sectorSize = (uint)decodedData.Length;
                }
            }

            ImageInfo.sectors = (ulong)sectorsData.Count;
            ImageInfo.mediaType = DecodeTeleDiskDiskType();

            stream.Close();

            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.imageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.imageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > (ulong)sectorsData.Count - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > (ulong)sectorsData.Count)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] data = new byte[1]; // To make compiler happy
            bool first = true;
            int dataPosition = 0;

            for(ulong i = sectorAddress; i < (sectorAddress + length); i++)
            {
                if(!sectorsData.ContainsKey((uint)i))
                    throw new ImageNotSupportedException(string.Format("Requested sector {0} not found", i));

                byte[] sector;

                if(!sectorsData.TryGetValue((uint)i, out sector))
                    throw new ImageNotSupportedException(string.Format("Error reading sector {0}", i));

                if(first)
                {
                    data = new byte[sector.Length];
                    Array.Copy(sector, data, sector.Length);
                    first = false;
                }
                else
                {
                    Array.Resize(ref data, dataPosition + sector.Length);
                    Array.Copy(sector, 0, data, dataPosition, sector.Length);
                }
                dataPosition += sector.Length;
            }

            return data;
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            return ReadSectors(sectorAddress, length);
        }

        public override string GetImageFormat()
        {
            return "Sydex TeleDisk";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.imageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.imageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            return !SectorsWhereCRCHasFailed.Contains(sectorAddress);
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++)
                if(SectorsWhereCRCHasFailed.Contains(sectorAddress))
                    FailingLBAs.Add(sectorAddress);

            return FailingLBAs.Count <= 0;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++)
                UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifyMediaImage()
        {
            return ADiskCRCHasFailed;
        }

        #region Private methods

        static ushort TeleDiskCRC(ushort crc, byte[] buffer)
        {
            int counter = 0;

            while(counter < buffer.Length)
            {
                crc ^= (ushort)((buffer[counter] & 0xFF) << 8);

                for(int i = 0; i < 8; i++)
                {
                    if((crc & 0x8000) > 0)
                        crc = (ushort)((crc << 1) ^ TeleDiskCRCPoly);
                    else
                        crc = (ushort)(crc << 1);
                }

                counter++;
            }

            return crc;
        }

        static byte[] DecodeTeleDiskData(byte sectorSize, byte encodingType, byte[] encodedData)
        {
            byte[] decodedData;
            switch(sectorSize)
            {
                case SectorSize128:
                    decodedData = new byte[128];
                    break;
                case SectorSize256:
                    decodedData = new byte[256];
                    break;
                case SectorSize512:
                    decodedData = new byte[512];
                    break;
                case SectorSize1K:
                    decodedData = new byte[1024];
                    break;
                case SectorSize2K:
                    decodedData = new byte[2048];
                    break;
                case SectorSize4K:
                    decodedData = new byte[4096];
                    break;
                case SectorSize8K:
                    decodedData = new byte[8192];
                    break;
                default:
                    throw new ImageNotSupportedException(string.Format("Sector size {0} is incorrect.", sectorSize));
            }

            switch(encodingType)
            {
                case dataBlockCopy:
                    Array.Copy(encodedData, decodedData, decodedData.Length);
                    break;
                case dataBlockPattern:
                    {
                        int ins = 0;
                        int outs = 0;
                        while(ins < encodedData.Length)
                        {
                            ushort repeatNumber;
                            byte[] repeatValue = new byte[2];

                            repeatNumber = BitConverter.ToUInt16(encodedData, ins);
                            Array.Copy(encodedData, ins + 2, repeatValue, 0, 2);
                            byte[] decodedPiece = new byte[repeatNumber * 2];
                            ArrayHelpers.ArrayFill(decodedPiece, repeatValue);
                            Array.Copy(decodedPiece, 0, decodedData, outs, decodedPiece.Length);
                            ins += 4;
                            outs += decodedPiece.Length;
                        }

                        DicConsole.DebugWriteLine("TeleDisk plugin", "(Block pattern decoder): Input data size: {0} bytes", encodedData.Length);
                        DicConsole.DebugWriteLine("TeleDisk plugin", "(Block pattern decoder): Processed input: {0} bytes", ins);
                        DicConsole.DebugWriteLine("TeleDisk plugin", "(Block pattern decoder): Output data size: {0} bytes", decodedData.Length);
                        DicConsole.DebugWriteLine("TeleDisk plugin", "(Block pattern decoder): Processed Output: {0} bytes", outs);
                        break;
                    }
                case dataBlockRLE:
                    {
                        int ins = 0;
                        int outs = 0;
                        while(ins < encodedData.Length)
                        {
                            byte Run;
                            byte Length;
                            byte Encoding;
                            byte[] Piece;

                            Encoding = encodedData[ins];
                            if(Encoding == 0x00)
                            {
                                Length = encodedData[ins + 1];
                                Array.Copy(encodedData, ins + 2, decodedData, outs, Length);
                                ins += (2 + Length);
                                outs += Length;
                            }
                            else
                            {
                                Length = (byte)(Encoding * 2);
                                Run = encodedData[ins + 1];
                                byte[] Part = new byte[Length];
                                Array.Copy(encodedData, ins + 2, Part, 0, Length);
                                Piece = new byte[Length * Run];
                                ArrayHelpers.ArrayFill(Piece, Part);
                                Array.Copy(Piece, 0, decodedData, outs, Piece.Length);
                                ins += (2 + Length);
                                outs += Piece.Length;
                            }
                        }

                        DicConsole.DebugWriteLine("TeleDisk plugin", "(RLE decoder): Input data size: {0} bytes", encodedData.Length);
                        DicConsole.DebugWriteLine("TeleDisk plugin", "(RLE decoder): Processed input: {0} bytes", ins);
                        DicConsole.DebugWriteLine("TeleDisk plugin", "(RLE decoder): Output data size: {0} bytes", decodedData.Length);
                        DicConsole.DebugWriteLine("TeleDisk plugin", "(RLE decoder): Processed Output: {0} bytes", outs);

                        break;
                    }
                default:
                    throw new ImageNotSupportedException(string.Format("Data encoding {0} is incorrect.", encodingType));
            }

            return decodedData;
        }

        MediaType DecodeTeleDiskDiskType()
        {
            switch(header.driveType)
            {
                case DriveType525DD:
                case DriveType525HD_DDDisk:
                case DriveType525HD:
                    {
                        switch(totalDiskSize)
                        {
                            case 163840:
                                {
                                    // Acorn disk uses 256 bytes/sector
                                    if(ImageInfo.sectorSize == 256)
                                        return MediaType.ACORN_525_SS_DD_40;
                                    // DOS disks use 512 bytes/sector
                                    return MediaType.DOS_525_SS_DD_8;
                                }
                            case 184320:
                                {
                                    // Atari disk uses 256 bytes/sector
                                    if(ImageInfo.sectorSize == 256)
                                        return MediaType.ATARI_525_DD;
                                    // DOS disks use 512 bytes/sector
                                    return MediaType.DOS_525_SS_DD_9;
                                }
                            case 327680:
                                {
                                    // Acorn disk uses 256 bytes/sector
                                    if(ImageInfo.sectorSize == 256)
                                        return MediaType.ACORN_525_SS_DD_80;
                                    // DOS disks use 512 bytes/sector
                                    return MediaType.DOS_525_DS_DD_8;
                                }
                            case 368640:
                                return MediaType.DOS_525_DS_DD_9;
                            case 1228800:
                                return MediaType.DOS_525_HD;
                            case 102400:
                                return MediaType.ACORN_525_SS_SD_40;
                            case 204800:
                                return MediaType.ACORN_525_SS_SD_80;
                            case 655360:
                                return MediaType.ACORN_525_DS_DD;
                            case 92160:
                                return MediaType.ATARI_525_SD;
                            case 133120:
                                return MediaType.ATARI_525_ED;
                            case 1310720:
                                return MediaType.NEC_525_HD;
                            case 1261568:
                                return MediaType.SHARP_525;
                            case 839680:
                                return MediaType.FDFORMAT_525_DD;
                            case 1304320:
                                return MediaType.ECMA_99_8;
                            case 1223424:
                                return MediaType.ECMA_99_15;
                            case 1061632:
                                return MediaType.ECMA_99_26;
                            case 80384:
                                return MediaType.ECMA_66;
                            case 325632:
                                return MediaType.ECMA_70;
                            case 653312:
                                return MediaType.ECMA_78;
                            case 737280:
                                return MediaType.ECMA_78_2;
                            default:
                                {
                                    DicConsole.DebugWriteLine("TeleDisk plugin", "Unknown 5,25\" disk with {0} bytes", totalDiskSize);
                                    return MediaType.Unknown;
                                }
                        }
                    }
                case DriveType35DD:
                case DriveType35ED:
                case DriveType35HD:
                    {
                        switch(totalDiskSize)
                        {
                            case 327680:
                                return MediaType.DOS_35_SS_DD_8;
                            case 368640:
                                return MediaType.DOS_35_SS_DD_9;
                            case 655360:
                                return MediaType.DOS_35_DS_DD_8;
                            case 737280:
                                return MediaType.DOS_35_DS_DD_9;
                            case 1474560:
                                return MediaType.DOS_35_HD;
                            case 2949120:
                                return MediaType.DOS_35_ED;
                            case 1720320:
                                return MediaType.DMF;
                            case 1763328:
                                return MediaType.DMF_82;
                            case 1884160: // Irreal size, seen as BIOS with TSR, 23 sectors/track
                            case 1860608: // Real data size, sum of all sectors
                                return MediaType.XDF_35;
                            case 819200:
                                return MediaType.CBM_35_DD;
                            case 901120:
                                return MediaType.CBM_AMIGA_35_DD;
                            case 1802240:
                                return MediaType.CBM_AMIGA_35_HD;
                            case 1310720:
                                return MediaType.NEC_35_HD_8;
                            case 1228800:
                                return MediaType.NEC_35_HD_15;
                            case 1261568:
                                return MediaType.SHARP_35;
                            default:
                                {
                                    DicConsole.DebugWriteLine("TeleDisk plugin", "Unknown 3,5\" disk with {0} bytes", totalDiskSize);
                                    return MediaType.Unknown;
                                }
                        }
                    }
                case DriveType8inch:
                    {
                        switch(totalDiskSize)
                        {
                            case 81664:
                                return MediaType.IBM23FD;
                            case 242944:
                                return MediaType.IBM33FD_128;
                            case 287488:
                                return MediaType.IBM33FD_256;
                            case 306432:
                                return MediaType.IBM33FD_512;
                            case 499200:
                                return MediaType.IBM43FD_128;
                            case 574976:
                                return MediaType.IBM43FD_256;
                            case 995072:
                                return MediaType.IBM53FD_256;
                            case 1146624:
                                return MediaType.IBM53FD_512;
                            case 1222400:
                                return MediaType.IBM53FD_1024;
                            case 256256:
                                // Same size, with same disk geometry, for DEC RX01, NEC and ECMA, return ECMA
                                return MediaType.ECMA_54;
                            case 512512:
                                {
                                    // DEC disk uses 256 bytes/sector
                                    if(ImageInfo.sectorSize == 256)
                                        return MediaType.RX02;
                                    // ECMA disks use 128 bytes/sector
                                    return MediaType.ECMA_59;
                                }
                            case 1261568:
                                return MediaType.NEC_8_DD;
                            case 1255168:
                                return MediaType.ECMA_69_8;
                            case 1177344:
                                return MediaType.ECMA_69_15;
                            case 1021696:
                                return MediaType.ECMA_69_26;
                            default:
                                {
                                    DicConsole.DebugWriteLine("TeleDisk plugin", "Unknown 8\" disk with {0} bytes", totalDiskSize);
                                    return MediaType.Unknown;
                                }
                        }
                    }
                default:
                    {
                        DicConsole.DebugWriteLine("TeleDisk plugin", "Unknown drive type {1} with {0} bytes", totalDiskSize, header.driveType);
                        return MediaType.Unknown;
                    }

            }
        }

        #endregion

        #region Unsupported features

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.mediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.mediaModel;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.mediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.mediaBarcode;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.mediaPartNumber;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.mediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.lastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.driveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.driveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.driveSerialNumber;
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

