// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DiscFerret.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages DiscFerret disk images.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    public class DiscFerret : ImagePlugin
    {
        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DfiBlockHeader
        {
            public ushort cylinder;
            public ushort head;
            public ushort sector;
            public uint length;
        }
        #endregion Internal Structures

        #region Internal Constants
        /// <summary>
        /// "DFER"
        /// </summary>
        const uint DfiMagic = 0x52454644;
        /// <summary>
        /// "DFE2"
        /// </summary>
        const uint DfiMagic2 = 0x32454644;
        #endregion Internal Constants

        #region Internal variables
        // TODO: These variables have been made public so create-sidecar can access to this information until I define an API >4.0
        public SortedDictionary<int, long> trackOffsets;
        public SortedDictionary<int, long> trackLengths;
        #endregion Internal variables

        public DiscFerret()
        {
            Name = "DiscFerret";
            PluginUUID = new Guid("70EA7B9B-5323-42EB-9B40-8DDA37C5EB4D");
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
            byte[] magic_b = new byte[4];
            Stream stream = imageFilter.GetDataForkStream();
            stream.Read(magic_b, 0, 4);
            uint magic = BitConverter.ToUInt32(magic_b, 0);

            return magic == DfiMagic || magic == DfiMagic2;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            byte[] magic_b = new byte[4];
            Stream stream = imageFilter.GetDataForkStream();
            stream.Read(magic_b, 0, 4);
            uint magic = BitConverter.ToUInt32(magic_b, 0);

            if(magic != DfiMagic && magic != DfiMagic2) return false;

            trackOffsets = new SortedDictionary<int, long>();
            trackLengths = new SortedDictionary<int, long>();
            int t = -1;
            ushort lastCylinder = 0, lastHead = 0;
            bool endOfTrack = false;
            long offset = 0;
            
            while(stream.Position < stream.Length)
            {
                long thisOffset = stream.Position;

                DfiBlockHeader blockHeader = new DfiBlockHeader();
                byte[] blk = new byte[Marshal.SizeOf(blockHeader)];
                stream.Read(blk, 0, Marshal.SizeOf(blockHeader));
                blockHeader = BigEndianMarshal.ByteArrayToStructureBigEndian<DfiBlockHeader>(blk);

                DicConsole.DebugWriteLine("DiscFerret plugin", "block@{0}.cylinder = {1}", thisOffset,
                    blockHeader.cylinder);
                DicConsole.DebugWriteLine("DiscFerret plugin", "block@{0}.head = {1}", thisOffset, blockHeader.head);
                DicConsole.DebugWriteLine("DiscFerret plugin", "block@{0}.sector = {1}", thisOffset,
                    blockHeader.sector);
                DicConsole.DebugWriteLine("DiscFerret plugin", "block@{0}.length = {1}", thisOffset,
                    blockHeader.length);
                
                if(stream.Position + blockHeader.length > stream.Length)
                {
                    DicConsole.DebugWriteLine("DiscFerret plugin", "Invalid track block found at {0}", thisOffset);
                    break;
                }

                stream.Position += blockHeader.length;
                
                if(blockHeader.cylinder > 0 && blockHeader.cylinder > lastCylinder)
                {
                    lastCylinder = blockHeader.cylinder;
                    lastHead = 0;
                    trackOffsets.Add(t, offset);
                    trackLengths.Add(t, thisOffset - offset + 1);
                    offset = thisOffset;
                    t++;
                }
                else if(blockHeader.head > 0 && blockHeader.head > lastHead)
                {
                    lastHead = blockHeader.head;
                    trackOffsets.Add(t, offset);
                    trackLengths.Add(t, thisOffset - offset + 1);
                    offset = thisOffset;
                    t++;
                }

                if(blockHeader.cylinder > ImageInfo.cylinders)
                    ImageInfo.cylinders = blockHeader.cylinder;
                if(blockHeader.head> ImageInfo.heads)
                    ImageInfo.heads= blockHeader.head;
            }

            ImageInfo.heads++;
            ImageInfo.cylinders++;

            ImageInfo.imageApplication = "DiscFerret";
            if(magic == DfiMagic2)
                ImageInfo.imageApplicationVersion = "2.0";
            else
                ImageInfo.imageApplicationVersion = "1.0";

            throw new NotImplementedException("Flux decoding is not yet implemented.");
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

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override string GetImageFormat()
        {
            return "DiscFerret";
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

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
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

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
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

        public override bool? VerifySector(ulong sectorAddress)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }
        #endregion Public methods

        #region Unsupported features
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

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
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

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifyMediaImage()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        #endregion Unsupported features
    }
}