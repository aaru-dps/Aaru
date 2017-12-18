// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HDCopy.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages floppy disk images created with HD-Copy
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
// Copyright © 2017 Michael Drüing
// ****************************************************************************/

/* Some information on the file format from Michal Necasek (www.os2museum.com):
 * 
 * The HD-Copy diskette image format was used by the eponymous DOS utility,
 * written by Oliver Fromme around 1995. The HD-Copy format is relatively
 * straightforward, supporting images with 512-byte sector size and uniform
 * sectors per track count. A basic form of run-length compression is also
 * supported, and empty/unused tracks aren't stored in the image. Images
 * with up to 82 cylinders are supported.
 *
 * No provision appears to be made for single-sided images. The disk image
 * is stored as a sequence of compressed tracks (where a track refers to only
 * one side of the disk), and individual tracks may be left out.
 *
 * The HD-Copy RLE compression works as follows. The image is divided into a
 * number of independent blocks, one per track. Each compressed block starts
 * with a header which contains the size of compressed data (16-bit little
 * endian) and the escape byte. Whenever the escape byte is encountered in the
 * byte stream, it is followed by a data byte and a count byte.
 *
 * Note that HD-Copy uses RLE compression for sequences of as few as three
 * bytes, even though that provides no benefit.
 *
 * It would be tempting to perform in-place decompression to save memory.
 * Unfortunately the simplistic RLE algorithm means the encoded data may be
 * larger than the decoded version, with unknown worst case behavior. Hence
 * the compressed data for a sector may not fit into a buffer the size of the
 * uncompressed sector.
 *
 * There is no signature, hence heuristics must be used to identify a HD-Copy
 * diskette image. Fortunately, the HD-Copy header is highly recognizable.
 */

using System;
using System.IO;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.CommonTypes;
using System.Linq;
using System.Text;
using DiscImageChef.Filters;
using System.Runtime.InteropServices;
using DiscImageChef.Decoders.Floppy;

namespace DiscImageChef.ImagePlugins
{
    public class HDCopy : ImagePlugin
    {
        #region Internal structures
        /// <summary>
        /// The global header of a HDCP image file
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HDCPFileHeader
        {
            /// <summary>
            /// Last cylinder (zero-based)
            /// </summary>
            public byte lastCylinder;

            /// <summary>
            /// Sectors per track
            /// </summary>
            public byte sectorsPerTrack;

            /// <summary>
            /// The track map. It contains one byte for each track.
            /// Up to 82 tracks (41 tracks * 2 sides) are supported.
            /// 0 means track is not present, 1 means it is present.
            /// The first 2 tracks are always present.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2 * 82)]
            public byte[] trackMap;
        }

        /// <summary>
        /// The header for a RLE-compressed block
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HDCPBlockHeader
        {
            /// <summary>
            /// The length of the compressed block, in bytes. Little-endian.
            /// </summary>
            public UInt16 length;

            /// <summary>
            /// The byte value used as RLE escape sequence
            /// </summary>
            public byte escape;
        }

        struct MediaTypeTableEntry
        {
            public byte tracks;
            public byte sectorsPerTrack;
            public MediaType mediaType;

            public MediaTypeTableEntry(byte _tracks, byte _sectorsPerTrack, MediaType _mediaType)
            {
                tracks = _tracks;
                sectorsPerTrack = _sectorsPerTrack;
                mediaType = _mediaType;
            }
        }

        #endregion

        #region Internal variables
        /// <summary>
        /// The HDCP file header after the image has been opened
        /// </summary>
        private HDCPFileHeader fileHeader;

        /// <summary>
        /// Every track that has been read is cached here
        /// </summary>
        private Dictionary<int, byte[]> trackCache = new Dictionary<int, byte[]>();

        /// <summary>
        /// The offset in the file where each track starts, or -1 if the track is not present
        /// </summary>
        private Dictionary<int, long> trackOffset = new Dictionary<int, long>();

        /// <summary>
        /// The ImageFilter we're reading from, after the file has been opened
        /// </summary>
        Filter hdcpImageFilter = null;
        #endregion

        #region Internal constants
        private readonly MediaTypeTableEntry[] mediaTypes =
        {
            new MediaTypeTableEntry(80, 8, MediaType.DOS_35_DS_DD_8),
            new MediaTypeTableEntry(80, 9, MediaType.DOS_35_DS_DD_9),
            new MediaTypeTableEntry(80, 18, MediaType.DOS_35_HD),
            new MediaTypeTableEntry(80, 36, MediaType.DOS_35_ED),
            new MediaTypeTableEntry(40, 8, MediaType.DOS_525_DS_DD_8),
            new MediaTypeTableEntry(40, 9, MediaType.DOS_525_DS_DD_9),
            new MediaTypeTableEntry(80, 15, MediaType.DOS_525_HD),
        };
        #endregion

        public HDCopy()
        {
            Name = "HD-Copy disk image";
            PluginUUID = new Guid("8D57483F-71A5-42EC-9B87-66AEC439C792");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplication = null;
            ImageInfo.imageApplicationVersion = null;
            ImageInfo.imageCreator = null;
            ImageInfo.imageComments = null;
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
            ImageInfo.driveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            HDCPFileHeader fheader;

            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if (stream.Length < 2 + 2 * 82)
                return false;

            byte[] header = new byte[2 + 2 * 82];
            stream.Read(header, 0, 2 + 2 * 82);

            IntPtr hdrPtr = Marshal.AllocHGlobal(2 + 2 * 82);
            Marshal.Copy(header, 0, hdrPtr, 2 + 2 * 82);
            fheader = (HDCPFileHeader)Marshal.PtrToStructure(hdrPtr, typeof(HDCPFileHeader));
            Marshal.FreeHGlobal(hdrPtr);

            /* Some sanity checks on the values we just read.
             * We know the image is from a DOS floppy disk, so assume
             * some sane cylinder and sectors-per-track count.
             */
            if ((fheader.sectorsPerTrack < 8) || (fheader.sectorsPerTrack > 40))
                return false;

            if ((fheader.lastCylinder < 37) || (fheader.lastCylinder >= 82))
                return false;

            // Validate the trackmap. First two tracks need to be present
            if ((fheader.trackMap[0] != 1) || (fheader.trackMap[1] != 1))
                return false;

            // all other tracks must be either present (=1) or absent (=0)
            for (int i = 0; i < 2 * 82; i++)
            {
                if (fheader.trackMap[i] > 1)
                    return false;
            }

            // TODO: validate the tracks
            // For now, having a valid header should be sufficient.
            return true;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            HDCPFileHeader fheader;
            long currentOffset;

            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] header = new byte[2 + 2 * 82];
            stream.Read(header, 0, 2 + 2 * 82);

            IntPtr hdrPtr = Marshal.AllocHGlobal(2 + 2 * 82);
            Marshal.Copy(header, 0, hdrPtr, 2 + 2 * 82);
            fheader = (HDCPFileHeader)Marshal.PtrToStructure(hdrPtr, typeof(HDCPFileHeader));
            Marshal.FreeHGlobal(hdrPtr);
            DicConsole.DebugWriteLine("HDCP plugin", "Detected HD-Copy image with {0} tracks and {1} sectors per track.", fheader.lastCylinder + 1, fheader.sectorsPerTrack);

            ImageInfo.cylinders = (uint)fheader.lastCylinder + 1;
            ImageInfo.sectorsPerTrack = fheader.sectorsPerTrack;
            ImageInfo.sectorSize = 512; // only 512 bytes per sector supported
            ImageInfo.heads = 2; // only 2-sided floppies are supported
            ImageInfo.sectors = 2 * ImageInfo.cylinders * ImageInfo.sectorsPerTrack;
            ImageInfo.imageSize = ImageInfo.sectors * ImageInfo.sectorSize;

            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;

            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.mediaType = GetMediaType();

            // the start offset of the track data
            currentOffset = 2 + 2 * 82;

            // build table of track offsets
            for (int i = 0; i < ImageInfo.cylinders * 2; i++)
            {
                if (fheader.trackMap[i] == 0)
                {
                    // track is not present in image
                    trackOffset[i] = -1;
                }
                else
                {
                    // track is present, read the block header
                    if (currentOffset + 3 >= stream.Length)
                        return false;

                    byte[] blkHeader = new byte[2];
                    short blkLength;
                    stream.Read(blkHeader, 0, 2);
                    blkLength = BitConverter.ToInt16(blkHeader, 0);

                    // assume block sizes are positive
                    if (blkLength < 0)
                        return false;

                    DicConsole.DebugWriteLine("HDCP plugin", "Track {0} offset 0x{1:x8}, size={2:x4}", i, currentOffset, blkLength);
                    trackOffset[i] = currentOffset;

                    currentOffset += 2 + blkLength;
                    // skip the block data
                    stream.Seek(blkLength, SeekOrigin.Current);
                }
            }

            // ensure that the last track is present completely
            if (currentOffset > stream.Length)
                return false;

            // save some variables for later use
            fileHeader = fheader;
            hdcpImageFilter = imageFilter;
            return true;
        }

        public override bool ImageHasPartitions()
        {
            return false;
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

        public override string GetImageFormat()
        {
            return "HD-Copy image";
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

        public override MediaType GetMediaType()
        {
            foreach (MediaTypeTableEntry ent in mediaTypes)
            {
                if ((ent.tracks == ImageInfo.cylinders) && (ent.sectorsPerTrack == ImageInfo.sectorsPerTrack))
                    return ent.mediaType;
            }

            return MediaType.Unknown;
        }

        private void ReadTrackIntoCache(Stream stream, int tracknum)
        {
            byte[] trackData = new byte[ImageInfo.sectorSize * ImageInfo.sectorsPerTrack];
            byte[] blkHeader = new byte[3];
            byte escapeByte;
            byte fillByte;
            byte fillCount;
            byte[] cBuffer;
            short compressedLength;

            // check that track is present
            if (trackOffset[tracknum] == -1)
                throw new InvalidDataException("Tried reading a track that is not present in image");

            stream.Seek(trackOffset[tracknum], SeekOrigin.Begin);

            // read the compressed track data
            stream.Read(blkHeader, 0, 3);
            compressedLength = (short)(BitConverter.ToInt16(blkHeader, 0) - 1);
            escapeByte = blkHeader[2];

            cBuffer = new byte[compressedLength];
            stream.Read(cBuffer, 0, compressedLength);

            // decompress the data
            int sIndex = 0; // source buffer position
            int dIndex = 0; // destination buffer position
            while (sIndex < compressedLength)
            {
                if (cBuffer[sIndex] == escapeByte)
                {
                    sIndex++; // skip over escape byte
                    fillByte = cBuffer[sIndex++]; // read fill byte
                    fillCount = cBuffer[sIndex++]; // read fill count
                    // fill destination buffer
                    for (int i = 0; i < fillCount; i++)
                    {
                        trackData[dIndex++] = fillByte;
                    }
                }
                else
                {
                    trackData[dIndex++] = cBuffer[sIndex++];
                }
            }

            // check that the number of bytes decompressed matches a whole track
            if (dIndex != ImageInfo.sectorSize * ImageInfo.sectorsPerTrack)
                throw new InvalidDataException("Track decompression yielded incomplete data");

            // store track in cache
            trackCache[tracknum] = trackData;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            int trackNum = (int)(sectorAddress / ImageInfo.sectorsPerTrack);
            int sectorOffset = (int)(sectorAddress % (ImageInfo.sectorsPerTrack * ImageInfo.sectorSize));
            byte[] result;

            if (sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if (trackNum > 2 * ImageInfo.cylinders)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            result = new byte[ImageInfo.sectorSize];
            if (trackOffset[trackNum] == -1)
            {
                // track is not present. Fill with zeroes.
                Array.Clear(result, 0, (int)ImageInfo.sectorSize);
            }
            else
            {
                // track is present in file, make sure it has been loaded
                if (!trackCache.ContainsKey(trackNum))
                    ReadTrackIntoCache(hdcpImageFilter.GetDataForkStream(), trackNum);

                Array.Copy(trackCache[trackNum], sectorOffset, result, 0, ImageInfo.sectorSize);
            }

            return result;
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            byte[] result = new byte[length * ImageInfo.sectorSize];

            if (sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            for (int i = 0; i < length; i++)
            {
                ReadSector(sectorAddress + (ulong)i).CopyTo(result, i * ImageInfo.sectorSize);
            }

            return result;
        }

        #region Unsupported features

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

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();
            for (ulong i = 0; i < ImageInfo.sectors; i++)
                UnknownLBAs.Add(i);
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
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
