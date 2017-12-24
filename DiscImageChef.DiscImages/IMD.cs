// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IMD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Sydex IMD disc images.
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
using System.Text;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class Imd : ImagePlugin
    {
        const byte SECTOR_CYLINDER_MAP_MASK = 0x80;
        const byte SECTOR_HEAD_MAP_MASK = 0x40;
        const byte COMMENT_END = 0x1A;
        const string REGEX_HEADER =
                "IMD (?<version>\\d.\\d+):\\s+(?<day>\\d+)\\/\\s*(?<month>\\d+)\\/(?<year>\\d+)\\s+(?<hour>\\d+):(?<minute>\\d+):(?<second>\\d+)\\r\\n"
            ;

        List<byte[]> sectorsData;

        public Imd()
        {
            Name = "Dunfield's IMD";
            PluginUuid = new Guid("0D67162E-38A3-407D-9B1A-CF40080A48CB");
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

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 31) return false;

            byte[] hdr = new byte[31];
            stream.Read(hdr, 0, 31);

            Regex hr = new Regex(REGEX_HEADER);
            Match hm = hr.Match(Encoding.ASCII.GetString(hdr));

            return hm.Success;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            MemoryStream cmt = new MemoryStream();
            stream.Seek(0x1F, SeekOrigin.Begin);
            for(uint i = 0; i < stream.Length; i++)
            {
                byte b = (byte)stream.ReadByte();
                if(b == 0x1A) break;

                cmt.WriteByte(b);
            }

            ImageInfo.ImageComments = StringHandlers.CToString(cmt.ToArray());
            sectorsData = new List<byte[]>();

            byte currentCylinder = 0;
            ImageInfo.Cylinders = 1;
            ImageInfo.Heads = 1;
            ulong currentLba = 0;

            TransferRate mode = TransferRate.TwoHundred;

            while(stream.Position + 5 < stream.Length)
            {
                mode = (TransferRate)stream.ReadByte();
                byte cylinder = (byte)stream.ReadByte();
                byte head = (byte)stream.ReadByte();
                byte spt = (byte)stream.ReadByte();
                byte n = (byte)stream.ReadByte();
                byte[] idmap = new byte[spt];
                byte[] cylmap = new byte[spt];
                byte[] headmap = new byte[spt];
                ushort[] bps = new ushort[spt];

                if(cylinder != currentCylinder)
                {
                    currentCylinder = cylinder;
                    ImageInfo.Cylinders++;
                }

                if((head & 1) == 1) ImageInfo.Heads = 2;

                stream.Read(idmap, 0, idmap.Length);
                if((head & SECTOR_CYLINDER_MAP_MASK) == SECTOR_CYLINDER_MAP_MASK) stream.Read(cylmap, 0, cylmap.Length);
                if((head & SECTOR_HEAD_MAP_MASK) == SECTOR_HEAD_MAP_MASK) stream.Read(headmap, 0, headmap.Length);
                if(n == 0xFF)
                {
                    byte[] bpsbytes = new byte[spt * 2];
                    stream.Read(bpsbytes, 0, bpsbytes.Length);
                    for(int i = 0; i < spt; i++) bps[i] = BitConverter.ToUInt16(bpsbytes, i * 2);
                }
                else for(int i = 0; i < spt; i++) bps[i] = (ushort)(128 << n);

                if(spt > ImageInfo.SectorsPerTrack) ImageInfo.SectorsPerTrack = spt;

                SortedDictionary<byte, byte[]> track = new SortedDictionary<byte, byte[]>();

                for(int i = 0; i < spt; i++)
                {
                    SectorType type = (SectorType)stream.ReadByte();
                    byte[] data = new byte[bps[i]];

                    // TODO; Handle disks with different bps in track 0
                    if(bps[i] > ImageInfo.SectorSize) ImageInfo.SectorSize = bps[i];

                    switch(type)
                    {
                        case SectorType.Unavailable:
                            if(!track.ContainsKey(idmap[i])) track.Add(idmap[i], data);
                            break;
                        case SectorType.Normal:
                        case SectorType.Deleted:
                        case SectorType.Error:
                        case SectorType.DeletedError:
                            stream.Read(data, 0, data.Length);
                            if(!track.ContainsKey(idmap[i])) track.Add(idmap[i], data);
                            ImageInfo.ImageSize += (ulong)data.Length;
                            break;
                        case SectorType.Compressed:
                        case SectorType.CompressedDeleted:
                        case SectorType.CompressedError:
                        case SectorType.CompressedDeletedError:
                            byte filling = (byte)stream.ReadByte();
                            ArrayHelpers.ArrayFill(data, filling);
                            if(!track.ContainsKey(idmap[i])) track.Add(idmap[i], data);
                            break;
                        default: throw new ImageNotSupportedException($"Invalid sector type {(byte)type}");
                    }
                }

                foreach(KeyValuePair<byte, byte[]> kvp in track)
                {
                    sectorsData.Add(kvp.Value);
                    currentLba++;
                }
            }

            ImageInfo.ImageApplication = "IMD";
            // TODO: The header is the date of dump or the date of the application compilation?
            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.ImageComments = StringHandlers.CToString(cmt.ToArray());
            ImageInfo.Sectors = currentLba;
            ImageInfo.MediaType = MediaType.Unknown;

            switch(mode)
            {
                case TransferRate.TwoHundred:
                case TransferRate.ThreeHundred:
                    if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 10 &&
                       ImageInfo.SectorSize == 256) ImageInfo.MediaType = MediaType.ACORN_525_SS_SD_40;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 10 &&
                            ImageInfo.SectorSize == 256) ImageInfo.MediaType = MediaType.ACORN_525_SS_SD_80;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 18 &&
                            ImageInfo.SectorSize == 128) ImageInfo.MediaType = MediaType.ATARI_525_SD;
                    break;
                case TransferRate.FiveHundred:
                    if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 32 && ImageInfo.SectorsPerTrack == 8 &&
                       ImageInfo.SectorSize == 319) ImageInfo.MediaType = MediaType.IBM23FD;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 73 && ImageInfo.SectorsPerTrack == 26 &&
                            ImageInfo.SectorSize == 128) ImageInfo.MediaType = MediaType.IBM23FD;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 77 && ImageInfo.SectorsPerTrack == 26 &&
                            ImageInfo.SectorSize == 128) ImageInfo.MediaType = MediaType.NEC_8_SD;
                    break;
                case TransferRate.TwoHundredMfm:
                case TransferRate.ThreeHundredMfm:
                    if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 8 &&
                       ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_SS_DD_8;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 9 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_SS_DD_9;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 8 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_DS_DD_8;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 9 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 18 &&
                            ImageInfo.SectorSize == 128) ImageInfo.MediaType = MediaType.ATARI_525_SD;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 26 &&
                            ImageInfo.SectorSize == 128) ImageInfo.MediaType = MediaType.ATARI_525_ED;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 18 &&
                            ImageInfo.SectorSize == 256) ImageInfo.MediaType = MediaType.ATARI_525_DD;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 16 &&
                            ImageInfo.SectorSize == 256) ImageInfo.MediaType = MediaType.ACORN_525_SS_DD_40;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 16 &&
                            ImageInfo.SectorSize == 256) ImageInfo.MediaType = MediaType.ACORN_525_SS_DD_80;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 18 &&
                            ImageInfo.SectorSize == 256) ImageInfo.MediaType = MediaType.ATARI_525_DD;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 10 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.RX50;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 9 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 8 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_DS_DD_8;
                    if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 9 &&
                       ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_SS_DD_9;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 8 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_SS_DD_8;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 5 &&
                            ImageInfo.SectorSize == 1024) ImageInfo.MediaType = MediaType.ACORN_35_DS_DD;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 82 && ImageInfo.SectorsPerTrack == 10 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.FDFORMAT_35_DD;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 70 && ImageInfo.SectorsPerTrack == 9 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.Apricot_35;
                    break;
                case TransferRate.FiveHundredMfm:
                    if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 18 &&
                       ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_HD;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 21 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DMF;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 82 && ImageInfo.SectorsPerTrack == 21 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DMF_82;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 23 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.XDF_35;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 15 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_HD;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 10 &&
                            ImageInfo.SectorSize == 1024) ImageInfo.MediaType = MediaType.ACORN_35_DS_HD;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 77 && ImageInfo.SectorsPerTrack == 8 &&
                            ImageInfo.SectorSize == 1024) ImageInfo.MediaType = MediaType.NEC_525_HD;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 9 &&
                            ImageInfo.SectorSize == 1024) ImageInfo.MediaType = MediaType.SHARP_525_9;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 10 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.ATARI_35_SS_DD;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 10 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.ATARI_35_DS_DD;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 11 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.ATARI_35_SS_DD_11;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 11 &&
                            ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.ATARI_35_DS_DD_11;
                    break;
                default:
                    ImageInfo.MediaType = MediaType.Unknown;
                    break;
            }

            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            DicConsole.VerboseWriteLine("IMD image contains a disk of type {0}", ImageInfo.MediaType);
            if(!string.IsNullOrEmpty(ImageInfo.ImageComments))
                DicConsole.VerboseWriteLine("IMD comments: {0}", ImageInfo.ImageComments);

            /*
            FileStream debugFs = new FileStream("debug.img", FileMode.CreateNew, FileAccess.Write);
            for(ulong i = 0; i < ImageInfo.sectors; i++)
                debugFs.Write(ReadSector(i), 0, (int)ImageInfo.sectorSize);
            debugFs.Dispose();
            */

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
            return null;
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

            MemoryStream buffer = new MemoryStream();
            for(int i = 0; i < length; i++)
                buffer.Write(sectorsData[(int)sectorAddress + i], 0, sectorsData[(int)sectorAddress + i].Length);

            return buffer.ToArray();
        }

        public override string GetImageFormat()
        {
            return "IMageDisk";
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

        enum TransferRate : byte
        {
            /// <summary>500 kbps in FM mode</summary>
            FiveHundred = 0,
            /// <summary>300 kbps in FM mode</summary>
            ThreeHundred = 1,
            /// <summary>250 kbps in FM mode</summary>
            TwoHundred = 2,
            /// <summary>500 kbps in MFM mode</summary>
            FiveHundredMfm = 3,
            /// <summary>300 kbps in MFM mode</summary>
            ThreeHundredMfm = 4,
            /// <summary>250 kbps in MFM mode</summary>
            TwoHundredMfm = 5
        }

        enum SectorType : byte
        {
            Unavailable = 0,
            Normal = 1,
            Compressed = 2,
            Deleted = 3,
            CompressedDeleted = 4,
            Error = 5,
            CompressedError = 6,
            DeletedError = 7,
            CompressedDeletedError = 8
        }
    }
}