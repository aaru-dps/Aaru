// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HDCopy.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
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
// Copyright © 2011-2018 Natalia Portillo
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public class HdCopy : IMediaImage
    {
        /// <summary>
        ///     The HDCP file header after the image has been opened
        /// </summary>
        HdcpFileHeader fileHeader;

        /// <summary>
        ///     The ImageFilter we're reading from, after the file has been opened
        /// </summary>
        IFilter hdcpImageFilter;
        ImageInfo imageInfo;

        /// <summary>
        ///     Every track that has been read is cached here
        /// </summary>
        Dictionary<int, byte[]> trackCache = new Dictionary<int, byte[]>();

        /// <summary>
        ///     The offset in the file where each track starts, or -1 if the track is not present
        /// </summary>
        Dictionary<int, long> trackOffset = new Dictionary<int, long>();

        public HdCopy()
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

        public ImageInfo Info => imageInfo;

        public string Name => "HD-Copy disk image";
        public Guid   Id   => new Guid("8D57483F-71A5-42EC-9B87-66AEC439C792");

        public string Format => "HD-Copy image";
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

            if(stream.Length < 2 + 2 * 82) return false;

            byte[] header = new byte[2 + 2 * 82];
            stream.Read(header, 0, 2 + 2 * 82);

            IntPtr hdrPtr = Marshal.AllocHGlobal(2 + 2 * 82);
            Marshal.Copy(header, 0, hdrPtr, 2 + 2 * 82);
            HdcpFileHeader fheader = (HdcpFileHeader)Marshal.PtrToStructure(hdrPtr, typeof(HdcpFileHeader));
            Marshal.FreeHGlobal(hdrPtr);

            /* Some sanity checks on the values we just read.
             * We know the image is from a DOS floppy disk, so assume
             * some sane cylinder and sectors-per-track count.
             */
            if(fheader.sectorsPerTrack < 8 || fheader.sectorsPerTrack > 40) return false;

            if(fheader.lastCylinder < 37 || fheader.lastCylinder >= 82) return false;

            // Validate the trackmap. First two tracks need to be present
            if(fheader.trackMap[0] != 1 || fheader.trackMap[1] != 1) return false;

            // all other tracks must be either present (=1) or absent (=0)
            for(int i = 0; i < 2 * 82; i++)
                if(fheader.trackMap[i] > 1)
                    return false;

            // TODO: validate the tracks
            // For now, having a valid header should be sufficient.
            return true;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] header = new byte[2 + 2 * 82];
            stream.Read(header, 0, 2 + 2 * 82);

            IntPtr hdrPtr = Marshal.AllocHGlobal(2 + 2 * 82);
            Marshal.Copy(header, 0, hdrPtr, 2 + 2 * 82);
            HdcpFileHeader fheader = (HdcpFileHeader)Marshal.PtrToStructure(hdrPtr, typeof(HdcpFileHeader));
            Marshal.FreeHGlobal(hdrPtr);
            DicConsole.DebugWriteLine("HDCP plugin",
                                      "Detected HD-Copy image with {0} tracks and {1} sectors per track.",
                                      fheader.lastCylinder + 1, fheader.sectorsPerTrack);

            imageInfo.Cylinders       = (uint)fheader.lastCylinder + 1;
            imageInfo.SectorsPerTrack = fheader.sectorsPerTrack;
            imageInfo.SectorSize      = 512; // only 512 bytes per sector supported
            imageInfo.Heads           = 2;   // only 2-sided floppies are supported
            imageInfo.Sectors         = 2 * imageInfo.Cylinders * imageInfo.SectorsPerTrack;
            imageInfo.ImageSize       = imageInfo.Sectors       * imageInfo.SectorSize;

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.MediaType = Geometry.GetMediaType(((ushort)imageInfo.Cylinders, 2,
                                                            (ushort)imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                            false));

            // the start offset of the track data
            long currentOffset = 2 + 2 * 82;

            // build table of track offsets
            for(int i = 0; i < imageInfo.Cylinders * 2; i++)
                if(fheader.trackMap[i] == 0)
                    trackOffset[i] = -1;
                else
                {
                    // track is present, read the block header
                    if(currentOffset + 3 >= stream.Length) return false;

                    byte[] blkHeader = new byte[2];
                    stream.Read(blkHeader, 0, 2);
                    short blkLength = BitConverter.ToInt16(blkHeader, 0);

                    // assume block sizes are positive
                    if(blkLength < 0) return false;

                    DicConsole.DebugWriteLine("HDCP plugin", "Track {0} offset 0x{1:x8}, size={2:x4}", i, currentOffset,
                                              blkLength);
                    trackOffset[i] = currentOffset;

                    currentOffset += 2 + blkLength;
                    // skip the block data
                    stream.Seek(blkLength, SeekOrigin.Current);
                }

            // ensure that the last track is present completely
            if(currentOffset > stream.Length) return false;

            // save some variables for later use
            fileHeader      = fheader;
            hdcpImageFilter = imageFilter;
            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            int trackNum     = (int)(sectorAddress / imageInfo.SectorsPerTrack);
            int sectorOffset = (int)(sectorAddress % imageInfo.SectorsPerTrack);

            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(trackNum > 2 * imageInfo.Cylinders)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            byte[] result = new byte[imageInfo.SectorSize];
            if(trackOffset[trackNum] == -1) Array.Clear(result, 0, (int)imageInfo.SectorSize);
            else
            {
                // track is present in file, make sure it has been loaded
                if(!trackCache.ContainsKey(trackNum)) ReadTrackIntoCache(hdcpImageFilter.GetDataForkStream(), trackNum);

                Array.Copy(trackCache[trackNum], sectorOffset * imageInfo.SectorSize, result, 0, imageInfo.SectorSize);
            }

            return result;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            byte[] result = new byte[length * imageInfo.SectorSize];

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            for(int i = 0; i < length; i++)
                ReadSector(sectorAddress + (ulong)i).CopyTo(result, i * imageInfo.SectorSize);

            return result;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
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

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
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

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
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

        public bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;

        void ReadTrackIntoCache(Stream stream, int tracknum)
        {
            byte[] trackData = new byte[imageInfo.SectorSize * imageInfo.SectorsPerTrack];
            byte[] blkHeader = new byte[3];

            // check that track is present
            if(trackOffset[tracknum] == -1)
                throw new InvalidDataException("Tried reading a track that is not present in image");

            stream.Seek(trackOffset[tracknum], SeekOrigin.Begin);

            // read the compressed track data
            stream.Read(blkHeader, 0, 3);
            short compressedLength = (short)(BitConverter.ToInt16(blkHeader, 0) - 1);
            byte  escapeByte       = blkHeader[2];

            byte[] cBuffer = new byte[compressedLength];
            stream.Read(cBuffer, 0, compressedLength);

            // decompress the data
            int sIndex = 0; // source buffer position
            int dIndex = 0; // destination buffer position
            while(sIndex < compressedLength)
                if(cBuffer[sIndex] == escapeByte)
                {
                    sIndex++; // skip over escape byte
                    byte fillByte  = cBuffer[sIndex++];
                    byte fillCount = cBuffer[sIndex++];
                    // fill destination buffer
                    for(int i = 0; i < fillCount; i++) trackData[dIndex++] = fillByte;
                }
                else
                    trackData[dIndex++] = cBuffer[sIndex++];

            // check that the number of bytes decompressed matches a whole track
            if(dIndex != imageInfo.SectorSize * imageInfo.SectorsPerTrack)
                throw new InvalidDataException("Track decompression yielded incomplete data");

            // store track in cache
            trackCache[tracknum] = trackData;
        }

        /// <summary>
        ///     The global header of a HDCP image file
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HdcpFileHeader
        {
            /// <summary>
            ///     Last cylinder (zero-based)
            /// </summary>
            public byte lastCylinder;

            /// <summary>
            ///     Sectors per track
            /// </summary>
            public byte sectorsPerTrack;

            /// <summary>
            ///     The track map. It contains one byte for each track.
            ///     Up to 82 tracks (41 tracks * 2 sides) are supported.
            ///     0 means track is not present, 1 means it is present.
            ///     The first 2 tracks are always present.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2 * 82)]
            public byte[] trackMap;
        }

        /// <summary>
        ///     The header for a RLE-compressed block
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HdcpBlockHeader
        {
            /// <summary>
            ///     The length of the compressed block, in bytes. Little-endian.
            /// </summary>
            public ushort length;

            /// <summary>
            ///     The byte value used as RLE escape sequence
            /// </summary>
            public byte escape;
        }
    }
}