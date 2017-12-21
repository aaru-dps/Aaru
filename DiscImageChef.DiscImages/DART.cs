// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DART.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Apple Disk Archival/Retrieval Tool format.
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
using System.Text;
using System.Text.RegularExpressions;
using Claunia.RsrcFork;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using DiscImageChef.DiscImages;

namespace DiscImageChef.DiscImages
{
    public class Dart : ImagePlugin
    {
        #region Internal constants
        // Disk types
        const byte DISK_MAC = 1;
        const byte DISK_LISA = 2;
        const byte DISK_APPLE2 = 3;
        const byte DISK_MAC_HD = 16;
        const byte DISK_DOS = 17;
        const byte DISK_DOS_HD = 18;

        // Compression types
        // "fast"
        const byte COMPRESS_RLE = 0;
        // "best"
        const byte COMPRESS_LZH = 1;
        // DART <= 1.4
        const byte COMPRESS_NONE = 2;

        // Valid sizes
        const short SIZE_LISA = 400;
        const short SIZE_MAC_SS = 400;
        const short SIZE_MAC = 800;
        const short SIZE_MAC_HD = 1440;
        const short SIZE_APPLE2 = 800;
        const short SIZE_DOS = 720;
        const short SIZE_DOS_HD = 1440;

        // bLength array sizes
        const int BLOCK_ARRAY_LEN_LOW = 40;
        const int BLOCK_ARRAY_LEN_HIGH = 72;

        const int SECTORS_PER_BLOCK = 40;
        const int SECTOR_SIZE = 512;
        const int TAG_SECTOR_SIZE = 12;
        const int DATA_SIZE = SECTORS_PER_BLOCK * SECTOR_SIZE;
        const int TAG_SIZE = SECTORS_PER_BLOCK * TAG_SECTOR_SIZE;
        const int BUFFER_SIZE = SECTORS_PER_BLOCK * SECTOR_SIZE + SECTORS_PER_BLOCK * TAG_SECTOR_SIZE;
        #endregion

        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DartHeader
        {
            public byte srcCmp;
            public byte srcType;
            public short srcSize;
        }
        #endregion

        // DART images are at most 1474560 bytes, so let's cache the whole
        byte[] dataCache;
        byte[] tagCache;
        uint dataChecksum;
        uint tagChecksum;

        public Dart()
        {
            Name = "Apple Disk Archival/Retrieval Tool";
            PluginUuid = new Guid("B3E06BF8-F98D-4F9B-BBE2-342C373BAF3E");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = false;
            ImageInfo.ImageHasSessions = false;
            ImageInfo.ImageVersion = null;
            ImageInfo.ImageApplication = null;
            ImageInfo.ImageApplicationVersion = null;
            ImageInfo.ImageCreator = null;
            ImageInfo.ImageComments = null;
            ImageInfo.MediaManufacturer = null;
            ImageInfo.MediaModel = null;
            ImageInfo.MediaSerialNumber = null;
            ImageInfo.MediaBarcode = null;
            ImageInfo.MediaPartNumber = null;
            ImageInfo.MediaSequence = 0;
            ImageInfo.LastMediaSequence = 0;
            ImageInfo.DriveManufacturer = null;
            ImageInfo.DriveModel = null;
            ImageInfo.DriveSerialNumber = null;
            ImageInfo.DriveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 84) return false;

            DartHeader header = new DartHeader();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] headerB = new byte[Marshal.SizeOf(header)];

            stream.Read(headerB, 0, Marshal.SizeOf(header));
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<DartHeader>(headerB);

            if(header.srcCmp > COMPRESS_NONE) return false;

            int expectedMaxSize = 84 + header.srcSize * 2 * 524;

            switch(header.srcType)
            {
                case DISK_MAC:
                    if(header.srcSize != SIZE_MAC_SS && header.srcSize != SIZE_MAC) return false;

                    break;
                case DISK_LISA:
                    if(header.srcSize != SIZE_LISA) return false;

                    break;
                case DISK_APPLE2:
                    if(header.srcSize != SIZE_APPLE2) return false;

                    break;
                case DISK_MAC_HD:
                    if(header.srcSize != SIZE_MAC_HD) return false;

                    expectedMaxSize += 64;
                    break;
                case DISK_DOS:
                    if(header.srcSize != SIZE_DOS) return false;

                    break;
                case DISK_DOS_HD:
                    if(header.srcSize != SIZE_DOS_HD) return false;

                    expectedMaxSize += 64;
                    break;
                default: return false;
            }

            if(stream.Length > expectedMaxSize) return false;

            return true;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 84) return false;

            DartHeader header = new DartHeader();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] headerB = new byte[Marshal.SizeOf(header)];

            stream.Read(headerB, 0, Marshal.SizeOf(header));
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<DartHeader>(headerB);

            if(header.srcCmp > COMPRESS_NONE) return false;

            int expectedMaxSize = 84 + header.srcSize * 2 * 524;

            switch(header.srcType)
            {
                case DISK_MAC:
                    if(header.srcSize != SIZE_MAC_SS && header.srcSize != SIZE_MAC) return false;

                    break;
                case DISK_LISA:
                    if(header.srcSize != SIZE_LISA) return false;

                    break;
                case DISK_APPLE2:
                    if(header.srcSize != DISK_APPLE2) return false;

                    break;
                case DISK_MAC_HD:
                    if(header.srcSize != SIZE_MAC_HD) return false;

                    expectedMaxSize += 64;
                    break;
                case DISK_DOS:
                    if(header.srcSize != SIZE_DOS) return false;

                    break;
                case DISK_DOS_HD:
                    if(header.srcSize != SIZE_DOS_HD) return false;

                    expectedMaxSize += 64;
                    break;
                default: return false;
            }

            if(stream.Length > expectedMaxSize) return false;

            short[] bLength;

            if(header.srcType == DISK_MAC_HD || header.srcType == DISK_DOS_HD) bLength = new short[BLOCK_ARRAY_LEN_HIGH];
            else bLength = new short[BLOCK_ARRAY_LEN_LOW];

            byte[] tmpShort;
            for(int i = 0; i < bLength.Length; i++)
            {
                tmpShort = new byte[2];
                stream.Read(tmpShort, 0, 2);
                bLength[i] = BigEndianBitConverter.ToInt16(tmpShort, 0);
            }

            byte[] temp;
            byte[] buffer;

            MemoryStream dataMs = new MemoryStream();
            MemoryStream tagMs = new MemoryStream();

            for(int i = 0; i < bLength.Length; i++)
                if(bLength[i] != 0)
                {
                    buffer = new byte[BUFFER_SIZE];
                    if(bLength[i] == -1)
                    {
                        stream.Read(buffer, 0, BUFFER_SIZE);
                        dataMs.Write(buffer, 0, DATA_SIZE);
                        tagMs.Write(buffer, DATA_SIZE, TAG_SIZE);
                    }
                    else if(header.srcCmp == COMPRESS_RLE)
                    {
                        temp = new byte[bLength[i] * 2];
                        stream.Read(temp, 0, temp.Length);
                        throw new ImageNotSupportedException("Compressed images not yet supported");
                    }
                    else
                    {
                        temp = new byte[bLength[i]];
                        stream.Read(temp, 0, temp.Length);
                        throw new ImageNotSupportedException("Compressed images not yet supported");
                    }
                }

            dataCache = dataMs.ToArray();
            if(header.srcType == DISK_LISA || header.srcType == DISK_MAC || header.srcType == DISK_APPLE2)
            {
                ImageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);
                tagCache = tagMs.ToArray();
            }

            try
            {
                if(imageFilter.HasResourceFork())
                {
                    ResourceFork rsrcFork = new ResourceFork(imageFilter.GetResourceForkStream());
                    // "vers"
                    if(rsrcFork.ContainsKey(0x76657273))
                    {
                        Resource versRsrc = rsrcFork.GetResource(0x76657273);
                        if(versRsrc != null)
                        {
                            byte[] vers = versRsrc.GetResource(versRsrc.GetIds()[0]);

                            if(vers != null)
                            {
                                Resources.Version version = new Resources.Version(vers);

                                string major;
                                string minor;
                                string release = null;
                                string dev = null;
                                string pre = null;

                                major = string.Format("{0}", version.MajorVersion);
                                minor = string.Format(".{0}", version.MinorVersion / 10);
                                if(version.MinorVersion % 10 > 0)
                                    release = string.Format(".{0}", version.MinorVersion % 10);
                                switch(version.DevStage)
                                {
                                    case Resources.Version.DevelopmentStage.Alpha:
                                        dev = "a";
                                        break;
                                    case Resources.Version.DevelopmentStage.Beta:
                                        dev = "b";
                                        break;
                                    case Resources.Version.DevelopmentStage.PreAlpha:
                                        dev = "d";
                                        break;
                                }

                                if(dev == null && version.PreReleaseVersion > 0) dev = "f";

                                if(dev != null) pre = string.Format("{0}", version.PreReleaseVersion);

                                ImageInfo.ImageApplicationVersion =
                                    string.Format("{0}{1}{2}{3}{4}", major, minor, release, dev, pre);
                                ImageInfo.ImageApplication = version.VersionString;
                                ImageInfo.ImageComments = version.VersionMessage;
                            }
                        }
                    }

                    // "dart"
                    if(rsrcFork.ContainsKey(0x44415254))
                    {
                        Resource dartRsrc = rsrcFork.GetResource(0x44415254);
                        if(dartRsrc != null)
                        {
                            string dArt = StringHandlers.PascalToString(dartRsrc.GetResource(dartRsrc.GetIds()[0]),
                                                                        Encoding.GetEncoding("macintosh"));
                            string dArtRegEx =
                                "(?<version>\\S+), tag checksum=\\$(?<tagchk>[0123456789ABCDEF]{8}), data checksum=\\$(?<datachk>[0123456789ABCDEF]{8})$";
                            Regex dArtEx = new Regex(dArtRegEx);
                            Match dArtMatch = dArtEx.Match(dArt);

                            if(dArtMatch.Success)
                            {
                                ImageInfo.ImageApplication = "DART";
                                ImageInfo.ImageApplicationVersion = dArtMatch.Groups["version"].Value;
                                dataChecksum = Convert.ToUInt32(dArtMatch.Groups["datachk"].Value, 16);
                                tagChecksum = Convert.ToUInt32(dArtMatch.Groups["tagchk"].Value, 16);
                            }
                        }
                    }

                    // "cksm"
                    if(rsrcFork.ContainsKey(0x434B534D))
                    {
                        Resource cksmRsrc = rsrcFork.GetResource(0x434B534D);
                        if(cksmRsrc != null)
                        {
                            if(cksmRsrc.ContainsId(1))
                            {
                                byte[] tagChk = cksmRsrc.GetResource(1);
                                tagChecksum = BigEndianBitConverter.ToUInt32(tagChk, 0);
                            }
                            if(cksmRsrc.ContainsId(2))
                            {
                                byte[] dataChk = cksmRsrc.GetResource(1);
                                dataChecksum = BigEndianBitConverter.ToUInt32(dataChk, 0);
                            }
                        }
                    }
                }
            }
            catch(InvalidCastException) { }

            DicConsole.DebugWriteLine("DART plugin", "Image application = {0} version {1}", ImageInfo.ImageApplication,
                                      ImageInfo.ImageApplicationVersion);

            ImageInfo.Sectors = (ulong)(header.srcSize * 2);
            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.SectorSize = SECTOR_SIZE;
            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.ImageSize = ImageInfo.Sectors * SECTOR_SIZE;
            if(header.srcCmp == COMPRESS_NONE) ImageInfo.ImageVersion = "1.4";
            else ImageInfo.ImageVersion = "1.5";

            switch(header.srcSize)
            {
                case SIZE_MAC_SS:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 10;
                    ImageInfo.MediaType = MediaType.AppleSonySS;
                    break;
                case SIZE_MAC:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 10;
                    ImageInfo.MediaType = MediaType.AppleSonyDS;
                    break;
                case SIZE_DOS:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 9;
                    ImageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                    break;
                case SIZE_MAC_HD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 18;
                    ImageInfo.MediaType = MediaType.DOS_35_HD;
                    break;
            }

            return true;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.SectorSize];

            Array.Copy(dataCache, (int)sectorAddress * ImageInfo.SectorSize, buffer, 0, length * ImageInfo.SectorSize);

            return buffer;
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(tag != SectorTagType.AppleSectorTag)
                throw new FeatureUnsupportedImageException(string.Format("Tag {0} not supported by image format", tag));

            if(tagCache == null || tagCache.Length == 0)
                throw new FeatureNotPresentImageException("Disk image does not have tags");

            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * TAG_SECTOR_SIZE];

            Array.Copy(tagCache, (int)sectorAddress * TAG_SECTOR_SIZE, buffer, 0, length * TAG_SECTOR_SIZE);

            return buffer;
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] data = ReadSectors(sectorAddress, length);
            byte[] tags = ReadSectorsTag(sectorAddress, length, SectorTagType.AppleSectorTag);
            byte[] buffer = new byte[data.Length + tags.Length];

            for(uint i = 0; i < length; i++)
            {
                Array.Copy(data, i * ImageInfo.SectorSize, buffer, i * (ImageInfo.SectorSize + TAG_SECTOR_SIZE),
                           ImageInfo.SectorSize);
                Array.Copy(tags, i * TAG_SECTOR_SIZE, buffer,
                           i * (ImageInfo.SectorSize + TAG_SECTOR_SIZE) + ImageInfo.SectorSize, TAG_SECTOR_SIZE);
            }

            return buffer;
        }

        public override bool ImageHasPartitions()
        {
            return false;
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

        public override string GetImageFormat()
        {
            return "Apple Disk Archival/Retrieval Tool";
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

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
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

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        #region Unsupported features
        public override byte[] ReadDiskTag(MediaTagType tag)
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

        public override string GetMediaManufacturer()
        {
            return null;
        }

        public override string GetMediaModel()
        {
            return null;
        }

        public override string GetMediaSerialNumber()
        {
            return null;
        }

        public override string GetMediaBarcode()
        {
            return null;
        }

        public override string GetMediaPartNumber()
        {
            return null;
        }

        public override int GetMediaSequence()
        {
            return 0;
        }

        public override int GetLastDiskSequence()
        {
            return 0;
        }

        public override string GetDriveManufacturer()
        {
            return null;
        }

        public override string GetDriveModel()
        {
            return null;
        }

        public override string GetDriveSerialNumber()
        {
            return null;
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

        public override bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < ImageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }
        #endregion
    }
}