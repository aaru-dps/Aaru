// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : QCOW.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages QEMU Copy-On-Write disk images.
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
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace DiscImageChef.DiscImages
{
    public class Qcow : IWritableImage
    {
        /// <summary>
        ///     Magic number: 'Q', 'F', 'I', 0xFB
        /// </summary>
        const uint QCOW_MAGIC = 0x514649FB;
        const uint  QCOW_VERSION         = 1;
        const uint  QCOW_ENCRYPTION_NONE = 0;
        const uint  QCOW_ENCRYPTION_AES  = 1;
        const ulong QCOW_COMPRESSED      = 0x8000000000000000;

        const int MAX_CACHE_SIZE = 16777216;

        const int                 MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;
        Dictionary<ulong, byte[]> clusterCache;
        int                       clusterSectors;
        int                       clusterSize;
        ImageInfo                 imageInfo;

        Stream imageStream;

        ulong                      l1Mask;
        int                        l1Shift;
        uint                       l1Size;
        ulong[]                    l1Table;
        ulong                      l2Mask;
        int                        l2Size;
        Dictionary<ulong, ulong[]> l2TableCache;
        int                        maxClusterCache;
        int                        maxL2TableCache;

        QCowHeader qHdr;

        Dictionary<ulong, byte[]> sectorCache;
        ulong                     sectorMask;
        FileStream                writingStream;

        public Qcow()
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

        public ImageInfo Info => imageInfo;

        public string Name => "QEMU Copy-On-Write disk image";
        public Guid   Id   => new Guid("A5C35765-9FE2-469D-BBBF-ACDEBDB7B954");

        public string Format => "QEMU Copy-On-Write";

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

            if(stream.Length < 512) return false;

            byte[] qHdrB = new byte[48];
            stream.Read(qHdrB, 0, 48);
            qHdr = BigEndianMarshal.ByteArrayToStructureBigEndian<QCowHeader>(qHdrB);

            return qHdr.magic == QCOW_MAGIC && qHdr.version == QCOW_VERSION;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] qHdrB = new byte[48];
            stream.Read(qHdrB, 0, 48);
            qHdr = BigEndianMarshal.ByteArrayToStructureBigEndian<QCowHeader>(qHdrB);

            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.magic = 0x{0:X8}",          qHdr.magic);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.version = {0}",             qHdr.version);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.backing_file_offset = {0}", qHdr.backing_file_offset);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.backing_file_size = {0}",   qHdr.backing_file_size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.mtime = {0}",               qHdr.mtime);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.size = {0}",                qHdr.size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.cluster_bits = {0}",        qHdr.cluster_bits);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l2_bits = {0}",             qHdr.l2_bits);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.padding = {0}",             qHdr.padding);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.crypt_method = {0}",        qHdr.crypt_method);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1_table_offset = {0}",     qHdr.l1_table_offset);

            if(qHdr.size <= 1) throw new ArgumentOutOfRangeException(nameof(qHdr.size), "Image size is too small");

            if(qHdr.cluster_bits < 9 || qHdr.cluster_bits > 16)
                throw new ArgumentOutOfRangeException(nameof(qHdr.cluster_bits),
                                                      "Cluster size must be between 512 bytes and 64 Kbytes");

            if(qHdr.l2_bits < 9 - 3 || qHdr.l2_bits > 16 - 3)
                throw new ArgumentOutOfRangeException(nameof(qHdr.l2_bits),
                                                      "L2 size must be between 512 bytes and 64 Kbytes");

            if(qHdr.crypt_method > QCOW_ENCRYPTION_AES)
                throw new ArgumentOutOfRangeException(nameof(qHdr.crypt_method), "Invalid encryption method");

            if(qHdr.crypt_method > QCOW_ENCRYPTION_NONE)
                throw new NotImplementedException("AES encrypted images not yet supported");

            if(qHdr.backing_file_offset != 0)
                throw new NotImplementedException("Differencing images not yet supported");

            int shift = qHdr.cluster_bits + qHdr.l2_bits;

            if(qHdr.size > ulong.MaxValue - (ulong)(1 << shift))
                throw new ArgumentOutOfRangeException(nameof(qHdr.size), "Image is too large");

            clusterSize    = 1 << qHdr.cluster_bits;
            clusterSectors = 1 << (qHdr.cluster_bits - 9);
            l1Size         = (uint)((qHdr.size + (ulong)(1 << shift) - 1) >> shift);
            l2Size         = 1 << qHdr.l2_bits;

            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.clusterSize = {0}",    clusterSize);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.clusterSectors = {0}", clusterSectors);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Size = {0}",         l1Size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l2Size = {0}",         l2Size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.sectors = {0}",        imageInfo.Sectors);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] l1TableB = new byte[l1Size * 8];
            stream.Seek((long)qHdr.l1_table_offset, SeekOrigin.Begin);
            stream.Read(l1TableB, 0, (int)l1Size * 8);
            l1Table = new ulong[l1Size];
            DicConsole.DebugWriteLine("QCOW plugin", "Reading L1 table");
            for(long i = 0; i < l1Table.LongLength; i++)
                l1Table[i] = BigEndianBitConverter.ToUInt64(l1TableB, (int)(i * 8));

            l1Mask = 0;
            int c = 0;
            l1Shift = qHdr.l2_bits + qHdr.cluster_bits;

            for(int i = 0; i < 64; i++)
            {
                l1Mask <<= 1;

                if(c >= 64 - l1Shift) continue;

                l1Mask += 1;
                c++;
            }

            l2Mask = 0;
            for(int i = 0; i < qHdr.l2_bits; i++) l2Mask = (l2Mask << 1) + 1;

            l2Mask <<= qHdr.cluster_bits;

            sectorMask = 0;
            for(int i = 0; i < qHdr.cluster_bits; i++) sectorMask = (sectorMask << 1) + 1;

            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Mask = {0:X}",     l1Mask);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Shift = {0}",      l1Shift);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l2Mask = {0:X}",     l2Mask);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.sectorMask = {0:X}", sectorMask);

            maxL2TableCache = MAX_CACHE_SIZE / (l2Size * 8);
            maxClusterCache = MAX_CACHE_SIZE / clusterSize;

            imageStream = stream;

            sectorCache  = new Dictionary<ulong, byte[]>();
            l2TableCache = new Dictionary<ulong, ulong[]>();
            clusterCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = qHdr.mtime > 0
                                                 ? DateHandlers.UnixUnsignedToDateTime(qHdr.mtime)
                                                 : imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle   = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors      = qHdr.size / 512;
            imageInfo.SectorSize   = 512;
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            imageInfo.MediaType    = MediaType.GENERIC_HDD;
            imageInfo.ImageSize    = qHdr.size;

            imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
            imageInfo.Heads           = 16;
            imageInfo.SectorsPerTrack = 63;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            // Check cache
            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            ulong byteAddress = sectorAddress * 512;

            ulong l1Off = (byteAddress & l1Mask) >> l1Shift;

            if((long)l1Off >= l1Table.LongLength)
                throw new ArgumentOutOfRangeException(nameof(l1Off),
                                                      $"Trying to read past L1 table, position {l1Off} of a max {l1Table.LongLength}");

            // TODO: Implement differential images
            if(l1Table[l1Off] == 0) return new byte[512];

            if(!l2TableCache.TryGetValue(l1Off, out ulong[] l2Table))
            {
                l2Table = new ulong[l2Size];
                imageStream.Seek((long)l1Table[l1Off], SeekOrigin.Begin);
                byte[] l2TableB = new byte[l2Size * 8];
                imageStream.Read(l2TableB, 0, l2Size * 8);
                DicConsole.DebugWriteLine("QCOW plugin", "Reading L2 table #{0}", l1Off);
                for(long i = 0; i < l2Table.LongLength; i++)
                    l2Table[i] = BigEndianBitConverter.ToUInt64(l2TableB, (int)(i * 8));

                if(l2TableCache.Count >= maxL2TableCache) l2TableCache.Clear();

                l2TableCache.Add(l1Off, l2Table);
            }

            ulong l2Off = (byteAddress & l2Mask) >> qHdr.cluster_bits;

            ulong offset = l2Table[l2Off];

            sector = new byte[512];

            if(offset != 0)
            {
                if(!clusterCache.TryGetValue(offset, out byte[] cluster))
                {
                    if((offset & QCOW_COMPRESSED) == QCOW_COMPRESSED)
                    {
                        ulong compSizeMask = (ulong)(1 << qHdr.cluster_bits) - 1;
                        compSizeMask <<= 63 - qHdr.cluster_bits;
                        ulong offMask = ~compSizeMask ^ QCOW_COMPRESSED;

                        ulong realOff  = offset & offMask;
                        ulong compSize = (offset & compSizeMask) >> (63 - qHdr.cluster_bits);

                        byte[] zCluster = new byte[compSize];
                        imageStream.Seek((long)realOff, SeekOrigin.Begin);
                        imageStream.Read(zCluster, 0, (int)compSize);

                        DeflateStream zStream =
                            new DeflateStream(new MemoryStream(zCluster), CompressionMode.Decompress);
                        cluster = new byte[clusterSize];
                        int read = zStream.Read(cluster, 0, clusterSize);

                        if(read != clusterSize)
                            throw new
                                IOException($"Unable to decompress cluster, expected {clusterSize} bytes got {read}");
                    }
                    else
                    {
                        cluster = new byte[clusterSize];
                        imageStream.Seek((long)offset, SeekOrigin.Begin);
                        imageStream.Read(cluster, 0, clusterSize);
                    }

                    if(clusterCache.Count >= maxClusterCache) clusterCache.Clear();

                    clusterCache.Add(offset, cluster);
                }

                Array.Copy(cluster, (int)(byteAddress & sectorMask), sector, 0, 512);
            }

            if(sectorCache.Count >= MAX_CACHED_SECTORS) sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
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
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".qcow", ".qc"};
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
            if(sectors * sectorSize / 65536 > uint.MaxValue)
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

            qHdr = new QCowHeader
            {
                magic           = QCOW_MAGIC,
                version         = QCOW_VERSION,
                size            = sectors * sectorSize,
                cluster_bits    = 12,
                l2_bits         = 9,
                l1_table_offset = (ulong)Marshal.SizeOf(typeof(QCowHeader))
            };

            int shift = qHdr.cluster_bits + qHdr.l2_bits;
            clusterSize    = 1 << qHdr.cluster_bits;
            clusterSectors = 1 << (qHdr.cluster_bits - 9);
            l1Size         = (uint)((qHdr.size + (ulong)(1 << shift) - 1) >> shift);
            l2Size         = 1 << qHdr.l2_bits;

            l1Table = new ulong[l1Size];

            l1Mask = 0;
            int c = 0;
            l1Shift = qHdr.l2_bits + qHdr.cluster_bits;

            for(int i = 0; i < 64; i++)
            {
                l1Mask <<= 1;

                if(c >= 64 - l1Shift) continue;

                l1Mask += 1;
                c++;
            }

            l2Mask = 0;
            for(int i = 0; i < qHdr.l2_bits; i++) l2Mask = (l2Mask << 1) + 1;

            l2Mask <<= qHdr.cluster_bits;

            sectorMask = 0;
            for(int i = 0; i < qHdr.cluster_bits; i++) sectorMask = (sectorMask << 1) + 1;

            byte[] empty = new byte[qHdr.l1_table_offset + l1Size * 8];
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
                byte[] l2TableB = new byte[l2Size * 8];
                writingStream.Seek(0, SeekOrigin.End);
                writingStream.Write(l2TableB, 0, l2TableB.Length);
            }

            writingStream.Position = (long)l1Table[l1Off];

            ulong l2Off = (byteAddress & l2Mask) >> qHdr.cluster_bits;

            writingStream.Seek((long)(l1Table[l1Off] + l2Off * 8), SeekOrigin.Begin);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] entry = new byte[8];
            writingStream.Read(entry, 0, 8);
            ulong offset = BigEndianBitConverter.ToUInt64(entry, 0);

            if(offset == 0)
            {
                offset = (ulong)writingStream.Length;
                byte[] cluster = new byte[clusterSize];
                entry = BigEndianBitConverter.GetBytes(offset);
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

            qHdr.mtime = (uint)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.magic),               0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.version),             0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.backing_file_offset), 0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.backing_file_size),   0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.mtime),               0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.size),                0, 8);
            writingStream.WriteByte(qHdr.cluster_bits);
            writingStream.WriteByte(qHdr.l2_bits);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.padding),         0, 2);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.crypt_method),    0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.l1_table_offset), 0, 8);

            writingStream.Seek((long)qHdr.l1_table_offset, SeekOrigin.Begin);
            for(long i = 0; i < l1Table.LongLength; i++)
                writingStream.Write(BigEndianBitConverter.GetBytes(l1Table[i]), 0, 8);

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

        /// <summary>
        ///     QCOW header, big-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QCowHeader
        {
            /// <summary>
            ///     <see cref="Qcow.QCOW_MAGIC" />
            /// </summary>
            public uint magic;
            /// <summary>
            ///     Must be 1
            /// </summary>
            public uint version;
            /// <summary>
            ///     Offset inside file to string containing backing file
            /// </summary>
            public ulong backing_file_offset;
            /// <summary>
            ///     Size of <see cref="backing_file_offset" />
            /// </summary>
            public uint backing_file_size;
            /// <summary>
            ///     Modification time
            /// </summary>
            public uint mtime;
            /// <summary>
            ///     Size in bytes
            /// </summary>
            public ulong size;
            /// <summary>
            ///     Cluster bits
            /// </summary>
            public byte cluster_bits;
            /// <summary>
            ///     L2 table bits
            /// </summary>
            public byte l2_bits;
            /// <summary>
            ///     Padding
            /// </summary>
            public ushort padding;
            /// <summary>
            ///     Encryption method
            /// </summary>
            public uint crypt_method;
            /// <summary>
            ///     Offset to L1 table
            /// </summary>
            public ulong l1_table_offset;
        }
    }
}