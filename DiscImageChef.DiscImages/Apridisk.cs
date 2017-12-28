// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Apridisk.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Apridisk images.
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
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class Apridisk : IMediaImage
    {
        readonly byte[] signature =
        {
            0x41, 0x43, 0x54, 0x20, 0x41, 0x70, 0x72, 0x69, 0x63, 0x6F, 0x74, 0x20, 0x64, 0x69, 0x73, 0x6B, 0x20, 0x69,
            0x6D, 0x61, 0x67, 0x65, 0x1A, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00
        };
        ImageInfo imageInfo;

        // Cylinder by head, sector data matrix
        byte[][][][] sectorsData;

        public Apridisk()
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

        public string Name => "ACT Apricot Disk Image";
        public Guid   Id   => new Guid("43408CF3-6DB3-449F-A779-2B0E497C5B14");

        public string Format => "ACT Apricot disk image";

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

            if(stream.Length < signature.Length) return false;

            byte[] sigB = new byte[signature.Length];
            stream.Read(sigB, 0, signature.Length);

            return sigB.SequenceEqual(signature);
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            // Skip signature
            stream.Seek(signature.Length, SeekOrigin.Begin);

            int totalCylinders = -1;
            int totalHeads     = -1;
            int maxSector      = -1;
            int recordSize     = Marshal.SizeOf(typeof(ApridiskRecord));

            // Count cylinders
            while(stream.Position < stream.Length)
            {
                byte[] recB = new byte[recordSize];
                stream.Read(recB, 0, recordSize);

                GCHandle       handle = GCHandle.Alloc(recB, GCHandleType.Pinned);
                ApridiskRecord record =
                    (ApridiskRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ApridiskRecord));
                handle.Free();

                switch(record.type)
                {
                    // Deleted record, just skip it
                    case RecordType.Deleted:
                        DicConsole.DebugWriteLine("Apridisk plugin", "Found deleted record at {0}", stream.Position);
                        stream.Seek(record.headerSize - recordSize + record.dataSize, SeekOrigin.Current);
                        break;
                    case RecordType.Comment:
                        DicConsole.DebugWriteLine("Apridisk plugin", "Found comment record at {0}", stream.Position);
                        stream.Seek(record.headerSize - recordSize, SeekOrigin.Current);
                        byte[] commentB = new byte[record.dataSize];
                        stream.Read(commentB, 0, commentB.Length);
                        imageInfo.Comments = StringHandlers.CToString(commentB);
                        DicConsole.DebugWriteLine("Apridisk plugin", "Comment: \"{0}\"", imageInfo.Comments);
                        break;
                    case RecordType.Creator:
                        DicConsole.DebugWriteLine("Apridisk plugin", "Found creator record at {0}", stream.Position);
                        stream.Seek(record.headerSize - recordSize, SeekOrigin.Current);
                        byte[] creatorB = new byte[record.dataSize];
                        stream.Read(creatorB, 0, creatorB.Length);
                        imageInfo.Creator = StringHandlers.CToString(creatorB);
                        DicConsole.DebugWriteLine("Apridisk plugin", "Creator: \"{0}\"", imageInfo.Creator);
                        break;
                    case RecordType.Sector:
                        if(record.compression != CompressType.Compressed &&
                           record.compression != CompressType.Uncompresed)
                            throw new
                                ImageNotSupportedException($"Found record with unknown compression type 0x{(ushort)record.compression:X4} at {stream.Position}");

                        DicConsole.DebugWriteLine("Apridisk plugin",
                                                  "Found {4} sector record at {0} for cylinder {1} head {2} sector {3}",
                                                  stream.Position, record.cylinder, record.head, record.sector,
                                                  record.compression == CompressType.Compressed
                                                      ? "compressed"
                                                      : "uncompressed");

                        if(record.cylinder > totalCylinders) totalCylinders = record.cylinder;
                        if(record.head     > totalHeads) totalHeads         = record.head;
                        if(record.sector   > maxSector) maxSector           = record.sector;

                        stream.Seek(record.headerSize - recordSize + record.dataSize, SeekOrigin.Current);
                        break;
                    default:
                        throw new
                            ImageNotSupportedException($"Found record with unknown type 0x{(uint)record.type:X8} at {stream.Position}");
                }
            }

            totalCylinders++;
            totalHeads++;

            if(totalCylinders <= 0 || totalHeads <= 0)
                throw new ImageNotSupportedException("No cylinders or heads found");

            sectorsData = new byte[totalCylinders][][][];
            // Total sectors per track
            uint[][] spts = new uint[totalCylinders][];

            imageInfo.Cylinders = (ushort)totalCylinders;
            imageInfo.Heads     = (byte)totalHeads;

            DicConsole.DebugWriteLine("Apridisk plugin",
                                      "Found {0} cylinders and {1} heads with a maximum sector number of {2}",
                                      totalCylinders, totalHeads, maxSector);

            // Create heads
            for(int i = 0; i < totalCylinders; i++)
            {
                sectorsData[i] = new byte[totalHeads][][];
                spts[i]        = new uint[totalHeads];

                for(int j = 0; j < totalHeads; j++) sectorsData[i][j] = new byte[maxSector + 1][];
            }

            imageInfo.SectorSize = uint.MaxValue;

            ulong headersizes = 0;

            // Read sectors
            stream.Seek(signature.Length, SeekOrigin.Begin);
            while(stream.Position < stream.Length)
            {
                byte[] recB = new byte[recordSize];
                stream.Read(recB, 0, recordSize);

                GCHandle       handle = GCHandle.Alloc(recB, GCHandleType.Pinned);
                ApridiskRecord record =
                    (ApridiskRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ApridiskRecord));
                handle.Free();

                switch(record.type)
                {
                    // Not sector record, just skip it
                    case RecordType.Deleted:
                    case RecordType.Comment:
                    case RecordType.Creator:
                        stream.Seek(record.headerSize    - recordSize + record.dataSize, SeekOrigin.Current);
                        headersizes += record.headerSize + record.dataSize;
                        break;
                    case RecordType.Sector:
                        stream.Seek(record.headerSize - recordSize, SeekOrigin.Current);

                        byte[] data = new byte[record.dataSize];
                        stream.Read(data, 0, data.Length);

                        spts[record.cylinder][record.head]++;
                        uint realLength = record.dataSize;

                        if(record.compression == CompressType.Compressed)
                            realLength =
                                Decompress(data, out sectorsData[record.cylinder][record.head][record.sector]);
                        else sectorsData[record.cylinder][record.head][record.sector] = data;

                        if(realLength < imageInfo.SectorSize) imageInfo.SectorSize = realLength;

                        headersizes += record.headerSize + record.dataSize;

                        break;
                }
            }

            DicConsole.DebugWriteLine("Apridisk plugin", "Found a minimum of {0} bytes per sector",
                                      imageInfo.SectorSize);

            // Count sectors per track
            uint spt = uint.MaxValue;
            for(ushort cyl = 0; cyl < imageInfo.Cylinders; cyl++)
            {
                for(ushort head = 0; head < imageInfo.Heads; head++)
                    if(spts[cyl][head]    < spt)
                        spt = spts[cyl][head];
            }

            imageInfo.SectorsPerTrack = spt;

            DicConsole.DebugWriteLine("Apridisk plugin", "Found a minimum of {0} sectors per track",
                                      imageInfo.SectorsPerTrack);

            imageInfo.MediaType =
                Geometry.GetMediaType(((ushort)imageInfo.Cylinders, (byte)imageInfo.Heads,
                                      (ushort)imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM, false));

            imageInfo.ImageSize            = (ulong)stream.Length - headersizes;
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = imageInfo.Cylinders * imageInfo.Heads * imageInfo.SectorsPerTrack;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;

            /*
            FileStream debugFs = new FileStream("debug.img", FileMode.CreateNew, FileAccess.Write);
            for(ulong i = 0; i < ImageInfo.sectors; i++)
                debugFs.Write(ReadSector(i), 0, (int)ImageInfo.sectorSize);
            debugFs.Dispose();
            */

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

            if(cylinder >= sectorsData.Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(head >= sectorsData[cylinder].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sector > sectorsData[cylinder][head].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            return sectorsData[cylinder][head][sector];
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                buffer.Write(sector, 0, sector.Length);
            }

            return buffer.ToArray();
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

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out                                   List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out                                               List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        static uint Decompress(byte[] compressed, out byte[] decompressed)
        {
            int          readp  = 0;
            int          cLen   = compressed.Length;
            MemoryStream buffer = new MemoryStream();

            uint uLen = 0;

            while(cLen >= 3)
            {
                ushort blklen = BitConverter.ToUInt16(compressed, readp);
                readp         += 2;

                for(int i = 0; i < blklen; i++) buffer.WriteByte(compressed[readp]);

                uLen += blklen;
                readp++;
                cLen -= 3;
            }

            decompressed = buffer.ToArray();
            return uLen;
        }

        (ushort cylinder, byte head, byte sector) LbaToChs(ulong lba)
        {
            ushort cylinder = (ushort)(lba / (imageInfo.Heads          * imageInfo.SectorsPerTrack));
            byte   head     = (byte)(lba   / imageInfo.SectorsPerTrack % imageInfo.Heads);
            byte   sector   = (byte)(lba   % imageInfo.SectorsPerTrack + 1);

            return (cylinder, head, sector);
        }

        enum RecordType : uint
        {
            Deleted = 0xE31D0000,
            Sector  = 0xE31D0001,
            Comment = 0xE31D0002,
            Creator = 0xE31D0003
        }

        enum CompressType : ushort
        {
            Uncompresed = 0x9E90,
            Compressed  = 0x3E5A
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ApridiskRecord
        {
            public RecordType   type;
            public CompressType compression;
            public ushort       headerSize;
            public uint         dataSize;
            public byte         head;
            public byte         sector;
            public ushort       cylinder;
        }
    }
}