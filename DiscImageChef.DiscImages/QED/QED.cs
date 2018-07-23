// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : QED.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages QEMU Enhanced Disk images.
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
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class Qed : IWritableImage
    {
        int                       clusterBits;
        Dictionary<ulong, byte[]> clusterCache;
        uint                      clusterSectors;
        ImageInfo                 imageInfo;
        Stream imageStream;
       ulong                      l1Mask;
        int                        l1Shift;
        ulong[]                    l1Table;
        ulong                      l2Mask;
        Dictionary<ulong, ulong[]> l2TableCache;
        uint                       maxClusterCache;
        uint                       maxL2TableCache;
        QedHeader qHdr;
        Dictionary<ulong, byte[]> sectorCache;
        ulong                     sectorMask;
        uint                      tableSize;
        FileStream                writingStream;

        public Qed()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Version               = "1",
                Application           = "QEMU",
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




        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadDiskTag(MediaTagType tag)
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

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[] { };
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.Unknown, MediaType.GENERIC_HDD, MediaType.FlashDrive, MediaType.CompactFlash,
                MediaType.CompactFlashType2, MediaType.PCCardTypeI, MediaType.PCCardTypeII, MediaType.PCCardTypeIII,
                MediaType.PCCardTypeIV
            };
        // TODO: Add cluster size option
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".qed"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            // TODO: Correct this calculation
            if(sectors * sectorSize / DEFAULT_CLUSTER_SIZE > uint.MaxValue)
            {
                ErrorMessage = "Too many sectors for selected cluster size";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try { writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            qHdr = new QedHeader
            {
                magic           = QED_MAGIC,
                cluster_size    = DEFAULT_CLUSTER_SIZE,
                table_size      = DEFAULT_TABLE_SIZE,
                header_size     = 1,
                l1_table_offset = DEFAULT_CLUSTER_SIZE,
                image_size      = sectors * sectorSize
            };

            clusterSectors = qHdr.cluster_size                   / 512;
            tableSize      = qHdr.cluster_size * qHdr.table_size / 8;

            l1Table = new ulong[tableSize];
            l1Mask  = 0;
            int c = 0;
            clusterBits = Ctz32(qHdr.cluster_size);
            l2Mask      = (tableSize - 1) << clusterBits;
            l1Shift     = clusterBits + Ctz32(tableSize);

            for(int i = 0; i < 64; i++)
            {
                l1Mask <<= 1;

                if(c >= 64 - l1Shift) continue;

                l1Mask += 1;
                c++;
            }

            sectorMask = 0;
            for(int i = 0; i < clusterBits; i++) sectorMask = (sectorMask << 1) + 1;

            byte[] empty = new byte[qHdr.l1_table_offset + tableSize * 8];
            writingStream.Write(empty, 0, empty.Length);

            IsWriting    = true;
            ErrorMessage = null;
            return true;
        }

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            ErrorMessage = "Writing media tags is not supported.";
            return false;
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(data.Length != imageInfo.SectorSize)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data)) return true;

            ulong byteAddress = sectorAddress * 512;

            ulong l1Off = (byteAddress & l1Mask) >> l1Shift;

            if((long)l1Off >= l1Table.LongLength)
                throw new ArgumentOutOfRangeException(nameof(l1Off),
                                                      $"Trying to write past L1 table, position {l1Off} of a max {l1Table.LongLength}");

            if(l1Table[l1Off] == 0)
            {
                writingStream.Seek(0, SeekOrigin.End);
                l1Table[l1Off] = (ulong)writingStream.Position;
                byte[] l2TableB = new byte[tableSize * 8];
                writingStream.Seek(0, SeekOrigin.End);
                writingStream.Write(l2TableB, 0, l2TableB.Length);
            }

            writingStream.Position = (long)l1Table[l1Off];

            ulong l2Off = (byteAddress & l2Mask) >> clusterBits;

            writingStream.Seek((long)(l1Table[l1Off] + l2Off * 8), SeekOrigin.Begin);

            byte[] entry = new byte[8];
            writingStream.Read(entry, 0, 8);
            ulong offset = BitConverter.ToUInt64(entry, 0);

            if(offset == 0)
            {
                offset = (ulong)writingStream.Length;
                byte[] cluster = new byte[qHdr.cluster_size];
                entry = BitConverter.GetBytes(offset);
                writingStream.Seek((long)(l1Table[l1Off] + l2Off * 8), SeekOrigin.Begin);
                writingStream.Write(entry, 0, 8);
                writingStream.Seek(0, SeekOrigin.End);
                writingStream.Write(cluster, 0, cluster.Length);
            }

            writingStream.Seek((long)(offset + (byteAddress & sectorMask)), SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";
            return true;
        }

        // TODO: This can be optimized
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(data.Length % imageInfo.SectorSize != 0)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data)) return true;

            for(uint i = 0; i < length; i++)
            {
                byte[] tmp = new byte[imageInfo.SectorSize];
                Array.Copy(data, i * imageInfo.SectorSize, tmp, 0, imageInfo.SectorSize);
                if(!WriteSector(tmp, sectorAddress + i)) return false;
            }

            ErrorMessage = "";
            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool SetTracks(List<Track> tracks)
        {
            ErrorMessage = "Unsupported feature";
            return false;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";
                return false;
            }

            byte[] hdr    = new byte[Marshal.SizeOf(typeof(QedHeader))];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(QedHeader)));
            Marshal.StructureToPtr(qHdr, hdrPtr, true);
            Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(hdr, 0, hdr.Length);

            writingStream.Seek((long)qHdr.l1_table_offset, SeekOrigin.Begin);
            for(long i = 0; i < l1Table.LongLength; i++) writingStream.Write(BitConverter.GetBytes(l1Table[i]), 0, 8);

            writingStream.Flush();
            writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            // Not stored in image
            return true;
        }

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware)
        {
            // Not supported
            return false;
        }

        public bool SetCicmMetadata(CICMMetadataType metadata)
        {
            // Not supported
            return false;
        }

        static bool IsPowerOfTwo(uint x)
        {
            while((x & 1) == 0 && x > 1) x >>= 1;

            return x == 1;
        }

        static int Ctz32(uint val)
        {
            int cnt = 0;
            if((val & 0xFFFF) == 0)
            {
                cnt +=  16;
                val >>= 16;
            }

            if((val & 0xFF) == 0)
            {
                cnt +=  8;
                val >>= 8;
            }

            if((val & 0xF) == 0)
            {
                cnt +=  4;
                val >>= 4;
            }

            if((val & 0x3) == 0)
            {
                cnt +=  2;
                val >>= 2;
            }

            if((val & 0x1) == 0)
            {
                cnt++;
                val >>= 1;
            }

            if((val & 0x1) == 0) cnt++;

            return cnt;
        }

        /// <summary>
        ///     QED header, big-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QedHeader
        {
            /// <summary>
            ///     <see cref="Qed.QED_MAGIC" />
            /// </summary>
            public uint magic;
            /// <summary>
            ///     Cluster size in bytes
            /// </summary>
            public uint cluster_size;
            /// <summary>
            ///     L1 and L2 table size in cluster
            /// </summary>
            public uint table_size;
            /// <summary>
            ///     Header size in clusters
            /// </summary>
            public uint header_size;
            /// <summary>
            ///     Incompatible features
            /// </summary>
            public ulong features;
            /// <summary>
            ///     Compatible features
            /// </summary>
            public ulong compat_features;
            /// <summary>
            ///     Self-resetting features
            /// </summary>
            public ulong autoclear_features;
            /// <summary>
            ///     Offset to L1 table
            /// </summary>
            public ulong l1_table_offset;
            /// <summary>
            ///     Image size
            /// </summary>
            public ulong image_size;
            /// <summary>
            ///     Offset inside file to string containing backing file
            /// </summary>
            public ulong backing_file_offset;
            /// <summary>
            ///     Size of <see cref="backing_file_offset" />
            /// </summary>
            public uint backing_file_size;
        }
    }
}