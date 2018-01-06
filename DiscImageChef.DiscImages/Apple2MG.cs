// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Apple2MG.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages XGS emulator disk images.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class Apple2Mg : ImagePlugin
    {
        /// <summary>
        ///     Magic number, "2IMG"
        /// </summary>
        const uint MAGIC = 0x474D4932;
        /// <summary>
        ///     Disk image created by ASIMOV2, "!nfc"
        /// </summary>
        const uint CREATOR_ASIMOV = 0x63666E21;
        /// <summary>
        ///     Disk image created by Bernie ][ the Rescue, "B2TR"
        /// </summary>
        const uint CREATOR_BERNIE = 0x52543242;
        /// <summary>
        ///     Disk image created by Catakig, "CTKG"
        /// </summary>
        const uint CREATOR_CATAKIG = 0x474B5443;
        /// <summary>
        ///     Disk image created by Sheppy's ImageMaker, "ShIm"
        /// </summary>
        const uint CREATOR_SHEPPY = 0x6D496853;
        /// <summary>
        ///     Disk image created by Sweet16, "WOOF"
        /// </summary>
        const uint CREATOR_SWEET = 0x464F4F57;
        /// <summary>
        ///     Disk image created by XGS, "XGS!"
        /// </summary>
        const uint CREATOR_XGS = 0x21534758;
        /// <summary>
        ///     Disk image created by CiderPress, "CdrP"
        /// </summary>
        const uint CREATOR_CIDER = 0x50726443;

        const    uint  LOCKED_DISK         = 0x80000000;
        const    uint  VALID_VOLUME_NUMBER = 0x00000100;
        const    uint  VOLUME_NUMBER_MASK  = 0x000000FF;
        readonly int[] deinterleave        = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};

        readonly int[] interleave = {0, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 15};

        Filter      a2MgImageFilter;
        byte[]      decodedImage;
        A2ImgHeader imageHeader;

        public Apple2Mg()
        {
            Name = "Apple 2IMG";
            PluginUuid = new Guid("CBAF8824-BA5F-415F-953A-19A03519B2D1");
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

            if(stream.Length < 65) return false;

            byte[] header = new byte[64];
            stream.Read(header, 0, 64);

            uint magic = BitConverter.ToUInt32(header, 0x00);
            if(magic != MAGIC) return false;

            uint dataoff = BitConverter.ToUInt32(header, 0x18);
            if(dataoff > stream.Length) return false;

            uint datasize = BitConverter.ToUInt32(header, 0x1C);
            // There seems to be incorrect endian in some images on the wild
            if(datasize == 0x00800C00) datasize = 0x000C8000;
            if(dataoff + datasize > stream.Length) return false;

            uint commentoff = BitConverter.ToUInt32(header, 0x20);
            if(commentoff > stream.Length) return false;

            uint commentsize = BitConverter.ToUInt32(header, 0x24);
            if(commentoff + commentsize > stream.Length) return false;

            uint creatoroff = BitConverter.ToUInt32(header, 0x28);
            if(creatoroff > stream.Length) return false;

            uint creatorsize = BitConverter.ToUInt32(header, 0x2C);
            return creatoroff + creatorsize <= stream.Length;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            imageHeader = new A2ImgHeader();

            byte[] header = new byte[64];
            stream.Read(header, 0, 64);
            byte[] magic = new byte[4];
            byte[] creator = new byte[4];

            Array.Copy(header, 0, magic, 0, 4);
            Array.Copy(header, 4, creator, 0, 4);

            imageHeader.Magic                 = BitConverter.ToUInt32(header, 0x00);
            imageHeader.Creator               = BitConverter.ToUInt32(header, 0x04);
            imageHeader.HeaderSize            = BitConverter.ToUInt16(header, 0x08);
            imageHeader.Version               = BitConverter.ToUInt16(header, 0x0A);
            imageHeader.ImageFormat           = (SectorOrder)BitConverter.ToUInt32(header, 0x0C);
            imageHeader.Flags                 = BitConverter.ToUInt32(header,              0x10);
            imageHeader.Blocks                = BitConverter.ToUInt32(header,              0x14);
            imageHeader.DataOffset            = BitConverter.ToUInt32(header,              0x18);
            imageHeader.DataSize              = BitConverter.ToUInt32(header,              0x1C);
            imageHeader.CommentOffset         = BitConverter.ToUInt32(header,              0x20);
            imageHeader.CommentSize           = BitConverter.ToUInt32(header,              0x24);
            imageHeader.CreatorSpecificOffset = BitConverter.ToUInt32(header,              0x28);
            imageHeader.CreatorSpecificSize   = BitConverter.ToUInt32(header,              0x2C);
            imageHeader.Reserved1             = BitConverter.ToUInt32(header,              0x30);
            imageHeader.Reserved2             = BitConverter.ToUInt32(header,              0x34);
            imageHeader.Reserved3             = BitConverter.ToUInt32(header,              0x38);
            imageHeader.Reserved4             = BitConverter.ToUInt32(header,              0x3C);

            if(imageHeader.DataSize == 0x00800C00)
            {
                imageHeader.DataSize = 0x000C8000;
                DicConsole.DebugWriteLine("2MG plugin", "Detected incorrect endian on data size field, correcting.");
            }

            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.magic = \"{0}\"", Encoding.ASCII.GetString(magic));
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.creator = \"{0}\"", Encoding.ASCII.GetString(creator));
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.headerSize = {0}", imageHeader.HeaderSize);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.version = {0}", imageHeader.Version);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.imageFormat = {0}", imageHeader.ImageFormat);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.flags = 0x{0:X8}", imageHeader.Flags);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.blocks = {0}", imageHeader.Blocks);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.dataOffset = 0x{0:X8}", imageHeader.DataOffset);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.dataSize = {0}", imageHeader.DataSize);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.commentOffset = 0x{0:X8}", imageHeader.CommentOffset);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.commentSize = {0}", imageHeader.CommentSize);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.creatorSpecificOffset = 0x{0:X8}",
                                      imageHeader.CreatorSpecificOffset);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.creatorSpecificSize = {0}",
                                      imageHeader.CreatorSpecificSize);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved1 = 0x{0:X8}", imageHeader.Reserved1);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved2 = 0x{0:X8}", imageHeader.Reserved2);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved3 = 0x{0:X8}", imageHeader.Reserved3);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved4 = 0x{0:X8}", imageHeader.Reserved4);

            if(imageHeader.DataSize    == 0 && imageHeader.Blocks == 0 &&
               imageHeader.ImageFormat != SectorOrder.ProDos) return false;

            byte[] tmp;
            int[]  offsets;

            switch(imageHeader.ImageFormat)
            {
                case SectorOrder.Nibbles:
                    tmp = new byte[imageHeader.DataSize];
                    stream.Seek(imageHeader.DataOffset, SeekOrigin.Begin);
                    stream.Read(tmp, 0, tmp.Length);
                    AppleNib    nibPlugin = new AppleNib();
                    ZZZNoFilter noFilter  = new ZZZNoFilter();
                    noFilter.Open(tmp);
                    nibPlugin.OpenImage(noFilter);
                    decodedImage         = nibPlugin.ReadSectors(0, (uint)nibPlugin.ImageInfo.Sectors);
                    ImageInfo.Sectors    = nibPlugin.ImageInfo.Sectors;
                    ImageInfo.SectorSize = nibPlugin.ImageInfo.SectorSize;
                    break;
                case SectorOrder.Dos when imageHeader.DataSize    == 143360:
                case SectorOrder.ProDos when imageHeader.DataSize == 143360:
                    stream.Seek(imageHeader.DataOffset, SeekOrigin.Begin);
                    tmp = new byte[imageHeader.DataSize];
                    stream.Read(tmp, 0, tmp.Length);
                    bool isDos = tmp[0x11001] == 17 && tmp[0x11002] < 16 && tmp[0x11027] <= 122 && tmp[0x11034] == 35 &&
                                 tmp[0x11035] == 16 && tmp[0x11036] == 0 && tmp[0x11037] == 1;
                    decodedImage = new byte[imageHeader.DataSize];
                    offsets      = imageHeader.ImageFormat == SectorOrder.Dos
                                       ? (isDos ? deinterleave : interleave)
                                       : (isDos ? interleave : deinterleave);
                    for(int t = 0; t < 35; t++)
                    {
                        for(int s = 0; s < 16; s++)
                            Array.Copy(tmp, t * 16 * 256 + s * 256, decodedImage, t * 16 * 256 + offsets[s] * 256, 256);
                    }

                    ImageInfo.Sectors    = 560;
                    ImageInfo.SectorSize = 256;
                    break;
                case SectorOrder.Dos when imageHeader.DataSize == 819200:
                    stream.Seek(imageHeader.DataOffset, SeekOrigin.Begin);
                    tmp = new byte[imageHeader.DataSize];
                    stream.Read(tmp, 0, tmp.Length);
                    decodedImage = new byte[imageHeader.DataSize];
                    offsets      = interleave;
                    for(int t = 0; t < 200; t++)
                    {
                        for(int s = 0; s < 16; s++)
                            Array.Copy(tmp, t * 16 * 256 + s * 256, decodedImage, t * 16 * 256 + offsets[s] * 256, 256);
                    }

                    ImageInfo.Sectors    = 1600;
                    ImageInfo.SectorSize = 512;
                    break;
                default:
                    decodedImage         = null;
                    ImageInfo.SectorSize = 512;
                    ImageInfo.Sectors    = imageHeader.DataSize / 512;
                    break;
            }

            ImageInfo.ImageSize = imageHeader.DataSize;

            switch(imageHeader.Creator)
            {
                case CREATOR_ASIMOV:
                    ImageInfo.ImageApplication = "ASIMOV2";
                    break;
                case CREATOR_BERNIE:
                    ImageInfo.ImageApplication = "Bernie ][ the Rescue";
                    break;
                case CREATOR_CATAKIG:
                    ImageInfo.ImageApplication = "Catakig";
                    break;
                case CREATOR_SHEPPY:
                    ImageInfo.ImageApplication = "Sheppy's ImageMaker";
                    break;
                case CREATOR_SWEET:
                    ImageInfo.ImageApplication = "Sweet16";
                    break;
                case CREATOR_XGS:
                    ImageInfo.ImageApplication = "XGS";
                    break;
                case CREATOR_CIDER:
                    ImageInfo.ImageApplication = "CiderPress";
                    break;
                default:
                    ImageInfo.ImageApplication = $"Unknown creator code \"{Encoding.ASCII.GetString(creator)}\"";
                    break;
            }

            ImageInfo.ImageVersion = imageHeader.Version.ToString();

            if(imageHeader.CommentOffset != 0 && imageHeader.CommentSize != 0)
            {
                stream.Seek(imageHeader.CommentOffset, SeekOrigin.Begin);

                byte[] comments = new byte[imageHeader.CommentSize];
                stream.Read(comments, 0, (int)imageHeader.CommentSize);
                ImageInfo.ImageComments = Encoding.ASCII.GetString(comments);
            }

            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.MediaType = GetMediaType();

            a2MgImageFilter = imageFilter;

            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            DicConsole.VerboseWriteLine("2MG image contains a disk of type {0}", ImageInfo.MediaType);
            if(!string.IsNullOrEmpty(ImageInfo.ImageComments))
                DicConsole.VerboseWriteLine("2MG comments: {0}", ImageInfo.ImageComments);

            switch(ImageInfo.MediaType)
            {
                case MediaType.Apple32SS:
                    ImageInfo.Cylinders = 35;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 13;
                    break;
                case MediaType.Apple32DS:
                    ImageInfo.Cylinders = 35;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 13;
                    break;
                case MediaType.Apple33SS:
                    ImageInfo.Cylinders = 35;
                    ImageInfo.Heads = 1;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.Apple33DS:
                    ImageInfo.Cylinders = 35;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.AppleSonySS:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 1;
                    // Variable sectors per track, this suffices
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.AppleSonyDS:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    // Variable sectors per track, this suffices
                    ImageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.DOS_35_HD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads     = 2;
                    ImageInfo.SectorsPerTrack = 18;
                    break;
            }

            return true;
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
            return "Apple 2IMG";
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

            if(decodedImage != null)
                Array.Copy(decodedImage, (long)(sectorAddress * ImageInfo.SectorSize), buffer, 0,
                           length                             * ImageInfo.SectorSize);
            else
            {
                Stream stream = a2MgImageFilter.GetDataForkStream();
                stream.Seek((long)(imageHeader.DataOffset + sectorAddress * ImageInfo.SectorSize), SeekOrigin.Begin);
                stream.Read(buffer, 0, (int)(length                       * ImageInfo.SectorSize));
            }

            return buffer;
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
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

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
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

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
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

        public override MediaType GetMediaType()
        {
            switch(ImageInfo.Sectors)
            {
                case 455:  return MediaType.Apple32SS;
                case 910:  return MediaType.Apple32DS;
                case 560:  return MediaType.Apple33SS;
                case 1120: return MediaType.Apple33DS;
                case 800:  return MediaType.AppleSonySS;
                case 1600: return MediaType.AppleSonyDS;
                case 2880: return MediaType.DOS_35_HD;
                default:   return MediaType.Unknown;
            }
        }

        enum SectorOrder : uint
        {
            Dos     = 0,
            ProDos  = 1,
            Nibbles = 2
        }

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        struct A2ImgHeader
        {
            /// <summary>
            ///     Offset 0x00, magic
            /// </summary>
            public uint Magic;
            /// <summary>
            ///     Offset 0x04, disk image creator ID
            /// </summary>
            public uint Creator;
            /// <summary>
            ///     Offset 0x08, header size, constant 0x0040
            /// </summary>
            public ushort HeaderSize;
            /// <summary>
            ///     Offset 0x0A, disk image version
            /// </summary>
            public ushort Version;
            /// <summary>
            ///     Offset 0x0C, disk image format
            /// </summary>
            public SectorOrder ImageFormat;
            /// <summary>
            ///     Offset 0x10, flags and volume number
            /// </summary>
            public uint Flags;
            /// <summary>
            ///     Offset 0x14, blocks for ProDOS, 0 otherwise
            /// </summary>
            public uint Blocks;
            /// <summary>
            ///     Offset 0x18, offset to data
            /// </summary>
            public uint DataOffset;
            /// <summary>
            ///     Offset 0x1C, data size in bytes
            /// </summary>
            public uint DataSize;
            /// <summary>
            ///     Offset 0x20, offset to optional comment
            /// </summary>
            public uint CommentOffset;
            /// <summary>
            ///     Offset 0x24, length of optional comment
            /// </summary>
            public uint CommentSize;
            /// <summary>
            ///     Offset 0x28, offset to creator specific chunk
            /// </summary>
            public uint CreatorSpecificOffset;
            /// <summary>
            ///     Offset 0x2C, creator specific chunk size
            /// </summary>
            public uint CreatorSpecificSize;
            /// <summary>
            ///     Offset 0x30, reserved, should be zero
            /// </summary>
            public uint Reserved1;
            /// <summary>
            ///     Offset 0x34, reserved, should be zero
            /// </summary>
            public uint Reserved2;
            /// <summary>
            ///     Offset 0x38, reserved, should be zero
            /// </summary>
            public uint Reserved3;
            /// <summary>
            ///     Offset 0x3C, reserved, should be zero
            /// </summary>
            public uint Reserved4;
        }
    }
}