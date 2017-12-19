// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SaveDskF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages IBM SaveDskF disk images.
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
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    public class SaveDskF : ImagePlugin
    {
        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SaveDskFHeader
        {
            /// <summary>0x00 magic number</summary>
            public ushort magic;
            /// <summary>0x02 media type from FAT</summary>
            public ushort mediaType;
            /// <summary>0x04 bytes per sector</summary>
            public ushort sectorSize;
            /// <summary>0x06 sectors per cluster - 1</summary>
            public byte clusterMask;
            /// <summary>0x07 log2(cluster / sector)</summary>
            public byte clusterShift;
            /// <summary>0x08 reserved sectors</summary>
            public ushort reservedSectors;
            /// <summary>0x0A copies of FAT</summary>
            public byte fatCopies;
            /// <summary>0x0B entries in root directory</summary>
            public ushort rootEntries;
            /// <summary>0x0D first cluster</summary>
            public ushort firstCluster;
            /// <summary>0x0F clusters present in image</summary>
            public ushort clustersCopied;
            /// <summary>0x11 sectors per FAT</summary>
            public byte sectorsPerFat;
            /// <summary>0x12 sector number of root directory</summary>
            public ushort rootDirectorySector;
            /// <summary>0x14 sum of all image bytes</summary>
            public uint checksum;
            /// <summary>0x18 cylinders</summary>
            public ushort cylinders;
            /// <summary>0x1A heads</summary>
            public ushort heads;
            /// <summary>0x1C sectors per track</summary>
            public ushort sectorsPerTrack;
            /// <summary>0x1E always zero</summary>
            public uint padding;
            /// <summary>0x22 sectors present in image</summary>
            public ushort sectorsCopied;
            /// <summary>0x24 offset to comment</summary>
            public ushort commentOffset;
            /// <summary>0x26 offset to data</summary>
            public ushort dataOffset;
        }
        #endregion Internal Structures

        #region Internal Constants
        const ushort SDF_MAGIC_OLD = 0x58AA;
        const ushort SDF_MAGIC = 0x59AA;
        const ushort SDF_MAGIC_COMPRESSED = 0x5AAA;
        #endregion Internal Constants

        #region Internal variables
        SaveDskFHeader header;
        long sectors;
        uint calculatedChk;
        byte[] decodedDisk;
        #endregion Internal variables

        public SaveDskF()
        {
            Name = "IBM SaveDskF";
            PluginUUID = new Guid("288CE058-1A51-4034-8C45-5A256CAE1461");
            ImageInfo = new ImageInfo()
            {
                readableSectorTags = new List<SectorTagType>(),
                readableMediaTags = new List<MediaTagType>(),
                imageHasPartitions = false,
                imageHasSessions = false,
                imageVersion = null,
                imageApplication = null,
                imageApplicationVersion = null,
                imageCreator = null,
                imageComments = null,
                mediaManufacturer = null,
                mediaModel = null,
                mediaSerialNumber = null,
                mediaBarcode = null,
                mediaPartNumber = null,
                mediaSequence = 0,
                lastMediaSequence = 0,
                driveManufacturer = null,
                driveModel = null,
                driveSerialNumber = null,
                driveFirmwareRevision = null
            };
        }

        #region Public methods
        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 41) return false;

            byte[] hdr = new byte[40];
            stream.Read(hdr, 0, 40);

            header = new SaveDskFHeader();
            IntPtr hdrPtr = Marshal.AllocHGlobal(40);
            Marshal.Copy(hdr, 0, hdrPtr, 40);
            header = (SaveDskFHeader)Marshal.PtrToStructure(hdrPtr, typeof(SaveDskFHeader));
            Marshal.FreeHGlobal(hdrPtr);

            return (header.magic == SDF_MAGIC || header.magic == SDF_MAGIC_COMPRESSED ||
                    header.magic == SDF_MAGIC_OLD) && header.fatCopies <= 2 && header.padding == 0 &&
                   header.commentOffset < stream.Length && header.dataOffset < stream.Length;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdr = new byte[40];
            byte[] cmt;

            stream.Read(hdr, 0, 40);
            header = new SaveDskFHeader();
            IntPtr hdrPtr = Marshal.AllocHGlobal(40);
            Marshal.Copy(hdr, 0, hdrPtr, 40);
            header = (SaveDskFHeader)Marshal.PtrToStructure(hdrPtr, typeof(SaveDskFHeader));
            Marshal.FreeHGlobal(hdrPtr);

            DicConsole.DebugWriteLine("SaveDskF plugin", "header.magic = 0x{0:X4}", header.magic);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.mediaType = 0x{0:X2}", header.mediaType);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.sectorSize = {0}", header.sectorSize);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.clusterMask = {0}", header.clusterMask);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.clusterShift = {0}", header.clusterShift);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.reservedSectors = {0}", header.reservedSectors);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.fatCopies = {0}", header.fatCopies);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.rootEntries = {0}", header.rootEntries);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.firstCluster = {0}", header.firstCluster);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.clustersCopied = {0}", header.clustersCopied);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.sectorsPerFat = {0}", header.sectorsPerFat);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.checksum = 0x{0:X8}", header.checksum);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.cylinders = {0}", header.cylinders);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.heads = {0}", header.heads);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.sectorsPerTrack = {0}", header.sectorsPerTrack);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.padding = {0}", header.padding);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.sectorsCopied = {0}", header.sectorsCopied);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.commentOffset = {0}", header.commentOffset);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.dataOffset = {0}", header.dataOffset);

            if(header.dataOffset == 0 && header.magic == SDF_MAGIC_OLD) header.dataOffset = 512;

            cmt = new byte[header.dataOffset - header.commentOffset];
            stream.Seek(header.commentOffset, SeekOrigin.Begin);
            stream.Read(cmt, 0, cmt.Length);
            if(cmt.Length > 1) ImageInfo.imageComments = StringHandlers.CToString(cmt, Encoding.GetEncoding("ibm437"));

            calculatedChk = 0;
            stream.Seek(0, SeekOrigin.Begin);

            int b = 0;
            do
            {
                b = stream.ReadByte();
                if(b >= 0) calculatedChk += (uint)b;
            }
            while(b >= 0);

            // In case there is omitted data
            sectors = (header.sectorsPerTrack) * header.heads * header.cylinders;

            DicConsole.DebugWriteLine("SaveDskF plugin", "Calculated checksum = 0x{0:X8}, {1}", calculatedChk,
                                      calculatedChk == header.checksum);

            ImageInfo.imageApplication = "SaveDskF";
            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = imageFilter.GetFilename();
            ImageInfo.imageSize = (ulong)(stream.Length - header.dataOffset);
            ImageInfo.sectors = (ulong)(header.sectorsPerTrack * header.heads * header.cylinders);
            ImageInfo.sectorSize = header.sectorSize;

            ImageInfo.mediaType = MediaType.Unknown;
            switch(header.cylinders)
            {
                case 40:
                    switch(header.heads)
                    {
                        case 1:
                            switch(header.sectorsPerTrack)
                            {
                                case 8:
                                    ImageInfo.mediaType = MediaType.DOS_525_SS_DD_8;
                                    break;
                                case 9:
                                    ImageInfo.mediaType = MediaType.DOS_525_SS_DD_9;
                                    break;
                            }

                            break;
                        case 2:
                            switch(header.sectorsPerTrack)
                            {
                                case 8:
                                    ImageInfo.mediaType = MediaType.DOS_525_DS_DD_8;
                                    break;
                                case 9:
                                    ImageInfo.mediaType = MediaType.DOS_525_DS_DD_9;
                                    break;
                            }

                            break;
                    }

                    break;
                case 70:
                    switch(header.heads)
                    {
                        case 1:
                            switch(header.sectorsPerTrack)
                            {
                                case 9:
                                    ImageInfo.mediaType = MediaType.Apricot_35;
                                    break;
                            }

                            break;
                    }

                    break;
                case 80:
                    switch(header.heads)
                    {
                        case 1:
                            switch(header.sectorsPerTrack)
                            {
                                case 8:
                                    ImageInfo.mediaType = MediaType.DOS_35_SS_DD_8;
                                    break;
                                case 9:
                                    ImageInfo.mediaType = MediaType.DOS_35_SS_DD_9;
                                    break;
                            }

                            break;
                        case 2:
                            switch(header.sectorsPerTrack)
                            {
                                case 8:
                                    ImageInfo.mediaType = MediaType.DOS_35_DS_DD_8;
                                    break;
                                case 9:
                                    ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;
                                    break;
                                case 15:
                                    ImageInfo.mediaType = MediaType.DOS_525_HD;
                                    break;
                                case 18:
                                    ImageInfo.mediaType = MediaType.DOS_35_HD;
                                    break;
                                case 23:
                                    ImageInfo.mediaType = MediaType.XDF_35;
                                    break;
                                case 36:
                                    ImageInfo.mediaType = MediaType.DOS_35_ED;
                                    break;
                            }

                            break;
                    }

                    break;
                default:
                    ImageInfo.mediaType = MediaType.Unknown;
                    break;
            }

            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;

            DicConsole.VerboseWriteLine("SaveDskF image contains a disk of type {0}", ImageInfo.mediaType);
            if(!string.IsNullOrEmpty(ImageInfo.imageComments))
                DicConsole.VerboseWriteLine("SaveDskF comments: {0}", ImageInfo.imageComments);

            // TODO: Support compressed images
            if(header.magic == SDF_MAGIC_COMPRESSED)
                throw new
                    FeatureSupportedButNotImplementedImageException("Compressed SaveDskF images are not supported.");

            // SaveDskF only ommits ending clusters, leaving no gaps behind, so reading all data we have...
            stream.Seek(header.dataOffset, SeekOrigin.Begin);
            decodedDisk = new byte[ImageInfo.sectors * ImageInfo.sectorSize];
            stream.Read(decodedDisk, 0, (int)(stream.Length - header.dataOffset));

            ImageInfo.cylinders = header.cylinders;
            ImageInfo.heads = header.heads;
            ImageInfo.sectorsPerTrack = header.sectorsPerTrack;

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

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs,
                                            out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs,
                                            out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifyMediaImage()
        {
            return calculatedChk == header.checksum;
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
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.sectorSize];

            Array.Copy(decodedDisk, (int)sectorAddress * ImageInfo.sectorSize, buffer, 0,
                       length * ImageInfo.sectorSize);

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "IBM SaveDskF";
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