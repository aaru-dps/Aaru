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
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public class Imd : IMediaImage
    {
        const byte SECTOR_CYLINDER_MAP_MASK = 0x80;
        const byte SECTOR_HEAD_MAP_MASK     = 0x40;
        const byte COMMENT_END              = 0x1A;
        const string REGEX_HEADER =
            @"IMD (?<version>\d.\d+):\s+(?<day>\d+)\/\s*(?<month>\d+)\/(?<year>\d+)\s+(?<hour>\d+):(?<minute>\d+):(?<second>\d+)\r\n";
        ImageInfo imageInfo;

        List<byte[]> sectorsData;

        public Imd()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Version               = null,
                Application           = null,
                ApplicationVersion    = null,
                Creator               = null,
                Comments              = null,
                MediaManufacturer     = null,
                MediaModel            = null,
                MediaSerialNumber     = null,
                MediaBarcode          = null,
                MediaPartNumber       = null,
                MediaSequence         = 0,
                LastMediaSequence     = 0,
                DriveManufacturer     = null,
                DriveModel            = null,
                DriveSerialNumber     = null,
                DriveFirmwareRevision = null
            };
        }

        public string    Name => "Dunfield's IMD";
        public Guid      Id   => new Guid("0D67162E-38A3-407D-9B1A-CF40080A48CB");
        public ImageInfo Info => imageInfo;

        public string Format => "IMageDisk";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
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

        public bool Open(IFilter imageFilter)
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

            imageInfo.Comments = StringHandlers.CToString(cmt.ToArray());
            sectorsData        = new List<byte[]>();

            byte currentCylinder = 0;
            imageInfo.Cylinders = 1;
            imageInfo.Heads     = 1;
            ulong currentLba = 0;

            TransferRate mode = TransferRate.TwoHundred;

            while(stream.Position + 5 < stream.Length)
            {
                mode = (TransferRate)stream.ReadByte();
                byte     cylinder = (byte)stream.ReadByte();
                byte     head     = (byte)stream.ReadByte();
                byte     spt      = (byte)stream.ReadByte();
                byte     n        = (byte)stream.ReadByte();
                byte[]   idmap    = new byte[spt];
                byte[]   cylmap   = new byte[spt];
                byte[]   headmap  = new byte[spt];
                ushort[] bps      = new ushort[spt];

                if(cylinder != currentCylinder)
                {
                    currentCylinder = cylinder;
                    imageInfo.Cylinders++;
                }

                if((head & 1) == 1) imageInfo.Heads = 2;

                stream.Read(idmap, 0, idmap.Length);
                if((head & SECTOR_CYLINDER_MAP_MASK) == SECTOR_CYLINDER_MAP_MASK) stream.Read(cylmap, 0, cylmap.Length);
                if((head & SECTOR_HEAD_MAP_MASK) == SECTOR_HEAD_MAP_MASK)
                    stream.Read(headmap, 0, headmap.Length);
                if(n == 0xFF)
                {
                    byte[] bpsbytes = new byte[spt * 2];
                    stream.Read(bpsbytes, 0, bpsbytes.Length);
                    for(int i = 0; i < spt; i++) bps[i] = BitConverter.ToUInt16(bpsbytes, i * 2);
                }
                else
                    for(int i = 0; i < spt; i++)
                        bps[i] = (ushort)(128 << n);

                if(spt > imageInfo.SectorsPerTrack) imageInfo.SectorsPerTrack = spt;

                SortedDictionary<byte, byte[]> track = new SortedDictionary<byte, byte[]>();

                for(int i = 0; i < spt; i++)
                {
                    SectorType type = (SectorType)stream.ReadByte();
                    byte[]     data = new byte[bps[i]];

                    // TODO; Handle disks with different bps in track 0
                    if(bps[i] > imageInfo.SectorSize) imageInfo.SectorSize = bps[i];

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
                            imageInfo.ImageSize += (ulong)data.Length;
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

            imageInfo.Application = "IMD";
            // TODO: The header is the date of dump or the date of the application compilation?
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Comments             = StringHandlers.CToString(cmt.ToArray());
            imageInfo.Sectors              = currentLba;
            imageInfo.MediaType            = MediaType.Unknown;

            MediaEncoding mediaEncoding = MediaEncoding.MFM;
            if(mode == TransferRate.TwoHundred || mode == TransferRate.ThreeHundred || mode == TransferRate.FiveHundred)
                mediaEncoding = MediaEncoding.FM;

            imageInfo.MediaType = Geometry.GetMediaType(((ushort)imageInfo.Cylinders, (byte)imageInfo.Heads,
                                                            (ushort)imageInfo.SectorsPerTrack, imageInfo.SectorSize,
                                                            mediaEncoding, false));

            switch(imageInfo.MediaType)
            {
                case MediaType.NEC_525_HD when mode == TransferRate.FiveHundredMfm:
                    imageInfo.MediaType = MediaType.NEC_35_HD_8;
                    break;
                case MediaType.DOS_525_HD when mode == TransferRate.FiveHundredMfm:
                    imageInfo.MediaType = MediaType.NEC_35_HD_15;
                    break;
                case MediaType.RX50 when mode == TransferRate.FiveHundredMfm:
                    imageInfo.MediaType = MediaType.ATARI_35_SS_DD;
                    break;
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            DicConsole.VerboseWriteLine("IMD image contains a disk of type {0}", imageInfo.MediaType);
            if(!string.IsNullOrEmpty(imageInfo.Comments))
                DicConsole.VerboseWriteLine("IMD comments: {0}", imageInfo.Comments);

            return true;
        }

        public bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            for(int i = 0; i < length; i++)
                buffer.Write(sectorsData[(int)sectorAddress + i], 0, sectorsData[(int)sectorAddress + i].Length);

            return buffer.ToArray();
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
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
            Unavailable            = 0,
            Normal                 = 1,
            Compressed             = 2,
            Deleted                = 3,
            CompressedDeleted      = 4,
            Error                  = 5,
            CompressedError        = 6,
            DeletedError           = 7,
            CompressedDeletedError = 8
        }
    }
}