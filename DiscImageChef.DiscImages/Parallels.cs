// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Parallels.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Parallels disk images.
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
    public class Parallels : IWritableImage
    {
        const uint PARALLELS_VERSION = 2;

        const uint PARALLELS_INUSE  = 0x746F6E59;
        const uint PARALLELS_CLOSED = 0x312E3276;

        const uint PARALLELS_EMPTY = 0x00000001;

        const    uint   MAX_CACHE_SIZE       = 16777216;
        const    uint   MAX_CACHED_SECTORS   = MAX_CACHE_SIZE / 512;
        const    uint   DEFAULT_CLUSTER_SIZE = 1048576;
        readonly byte[] parallelsExtMagic    =
            {0x57, 0x69, 0x74, 0x68, 0x6F, 0x75, 0x46, 0x72, 0x65, 0x53, 0x70, 0x61, 0x63, 0x45, 0x78, 0x74};
        readonly byte[] parallelsMagic =
            {0x57, 0x69, 0x74, 0x68, 0x6F, 0x75, 0x74, 0x46, 0x72, 0x65, 0x65, 0x53, 0x70, 0x61, 0x63, 0x65};
        uint[] bat;
        uint   clusterBytes;
        long   currentWritingPosition;
        long   dataOffset;
        bool   empty;

        bool            extended;
        ImageInfo       imageInfo;
        Stream          imageStream;
        ParallelsHeader pHdr;

        Dictionary<ulong, byte[]> sectorCache;
        FileStream                writingStream;

        public Parallels()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Version               = "2",
                Application           = "Parallels",
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

        public string    Name => "Parallels disk image";
        public Guid      Id   => new Guid("E314DE35-C103-48A3-AD36-990F68523C46");
        public ImageInfo Info => imageInfo;

        public string Format => "Parallels";

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

            byte[] pHdrB = new byte[Marshal.SizeOf(pHdr)];
            stream.Read(pHdrB, 0, Marshal.SizeOf(pHdr));
            pHdr             = new ParallelsHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pHdr));
            Marshal.Copy(pHdrB, 0, headerPtr, Marshal.SizeOf(pHdr));
            pHdr = (ParallelsHeader)Marshal.PtrToStructure(headerPtr, typeof(ParallelsHeader));
            Marshal.FreeHGlobal(headerPtr);

            return parallelsMagic.SequenceEqual(pHdr.magic) || parallelsExtMagic.SequenceEqual(pHdr.magic);
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] pHdrB = new byte[Marshal.SizeOf(pHdr)];
            stream.Read(pHdrB, 0, Marshal.SizeOf(pHdr));
            pHdr             = new ParallelsHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pHdr));
            Marshal.Copy(pHdrB, 0, headerPtr, Marshal.SizeOf(pHdr));
            pHdr = (ParallelsHeader)Marshal.PtrToStructure(headerPtr, typeof(ParallelsHeader));
            Marshal.FreeHGlobal(headerPtr);

            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.magic = {0}",
                                      StringHandlers.CToString(pHdr.magic));
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.version = {0}",      pHdr.version);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.heads = {0}",        pHdr.heads);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.cylinders = {0}",    pHdr.cylinders);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.cluster_size = {0}", pHdr.cluster_size);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.bat_entries = {0}",  pHdr.bat_entries);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.sectors = {0}",      pHdr.sectors);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.in_use = 0x{0:X8}",  pHdr.in_use);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.data_off = {0}",     pHdr.data_off);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.flags = {0}",        pHdr.flags);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.ext_off = {0}",      pHdr.ext_off);

            extended = parallelsExtMagic.SequenceEqual(pHdr.magic);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.extended = {0}", extended);

            DicConsole.DebugWriteLine("Parallels plugin", "Reading BAT");
            bat         = new uint[pHdr.bat_entries];
            byte[] batB = new byte[pHdr.bat_entries * 4];
            stream.Read(batB, 0, batB.Length);
            for(int i = 0; i < bat.Length; i++) bat[i] = BitConverter.ToUInt32(batB, i * 4);

            clusterBytes                     = pHdr.cluster_size * 512;
            if(pHdr.data_off > 0) dataOffset = pHdr.data_off     * 512;
            else
                dataOffset =
                    (stream.Position / clusterBytes + stream.Position % clusterBytes) * clusterBytes;

            sectorCache = new Dictionary<ulong, byte[]>();

            empty = (pHdr.flags & PARALLELS_EMPTY) == PARALLELS_EMPTY;

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = pHdr.sectors;
            imageInfo.SectorSize           = 512;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.ImageSize            = pHdr.sectors * 512;
            imageInfo.Cylinders            = pHdr.cylinders;
            imageInfo.Heads                = pHdr.heads;
            imageInfo.SectorsPerTrack      = (uint)(imageInfo.Sectors / imageInfo.Cylinders / imageInfo.Heads);
            imageStream                    = stream;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(empty) return new byte[512];

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            ulong index  = sectorAddress / pHdr.cluster_size;
            ulong secOff = sectorAddress % pHdr.cluster_size;

            uint  batOff = bat[index];
            ulong imageOff;

            if(batOff == 0) return new byte[512];

            if(extended) imageOff = batOff * clusterBytes;
            else imageOff         = batOff * 512;

            byte[] cluster = new byte[clusterBytes];
            imageStream.Seek((long)imageOff, SeekOrigin.Begin);
            imageStream.Read(cluster, 0, (int)clusterBytes);
            sector = new byte[512];
            Array.Copy(cluster, (int)(secOff * 512), sector, 0, 512);

            if(sectorCache.Count > MAX_CACHED_SECTORS) sectorCache.Clear();

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

            if(empty) return new byte[512 * length];

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

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[] { };
        public IEnumerable<MediaType>     SupportedMediaTypes => new[] {MediaType.Unknown, MediaType.GENERIC_HDD};
        // TODO: Add cluster size option
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".hdd"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        // TODO: Support extended
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

            if(sectors * sectorSize / DEFAULT_CLUSTER_SIZE > uint.MaxValue)
            {
                ErrorMessage = "Too many sectors for selected cluster size";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try { writingStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            uint batEntries = (uint)(sectors * sectorSize /
                                     DEFAULT_CLUSTER_SIZE);
            if(sectors * sectorSize %
               DEFAULT_CLUSTER_SIZE > 0) batEntries++;
            uint headerSectors = (uint)Marshal.SizeOf(typeof(ParallelsHeader)) + batEntries * 4;
            if((uint)Marshal.SizeOf(typeof(ParallelsHeader))                   + batEntries % 4 > 0) headerSectors++;

            pHdr = new ParallelsHeader
            {
                magic        = parallelsMagic,
                version      = PARALLELS_VERSION,
                sectors      = sectors,
                in_use       = PARALLELS_CLOSED,
                bat_entries  = batEntries,
                data_off     = headerSectors,
                cluster_size = DEFAULT_CLUSTER_SIZE / 512
            };

            bat                    = new uint[batEntries];
            currentWritingPosition = headerSectors * 512;

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

            if(data.Length != 512)
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

            ulong index  = sectorAddress / pHdr.cluster_size;
            ulong secOff = sectorAddress % pHdr.cluster_size;

            uint batOff = bat[index];

            if(batOff == 0)
            {
                batOff                 =  (uint)(currentWritingPosition / 512);
                bat[index]             =  batOff;
                currentWritingPosition += pHdr.cluster_size * 512;
            }

            ulong imageOff = batOff * 512;

            writingStream.Seek((long)imageOff,     SeekOrigin.Begin);
            writingStream.Seek((long)secOff * 512, SeekOrigin.Current);
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

            if(data.Length % 512 != 0)
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
                byte[] tmp = new byte[512];
                Array.Copy(data, i * 512, tmp, 0, 512);
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

            byte[] hdr    = new byte[Marshal.SizeOf(typeof(ParallelsHeader))];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ParallelsHeader)));
            Marshal.StructureToPtr(pHdr, hdrPtr, true);
            Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(hdr, 0, hdr.Length);

            for(long i = 0; i < bat.LongLength; i++) writingStream.Write(BitConverter.GetBytes(bat[i]), 0, 4);

            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            pHdr.cylinders = cylinders;
            pHdr.heads     = heads;
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

        /// <summary>
        ///     Parallels disk image header, little-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ParallelsHeader
        {
            /// <summary>
            ///     Magic, <see cref="Parallels.parallelsMagic" /> or <see cref="Parallels.parallelsExtMagic" />
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] magic;
            /// <summary>
            ///     Version
            /// </summary>
            public uint version;
            /// <summary>
            ///     Disk geometry parameter
            /// </summary>
            public uint heads;
            /// <summary>
            ///     Disk geometry parameter
            /// </summary>
            public uint cylinders;
            /// <summary>
            ///     Cluser size in sectors
            /// </summary>
            public uint cluster_size;
            /// <summary>
            ///     Entries in BAT (clusters in image)
            /// </summary>
            public uint bat_entries;
            /// <summary>
            ///     Disk size in sectors
            /// </summary>
            public ulong sectors;
            /// <summary>
            ///     Set to <see cref="Parallels.PARALLELS_INUSE" /> if image is opened by any software,
            ///     <see cref="Parallels.PARALLELS_CLOSED" /> if not, and 0 if old version
            /// </summary>
            public uint in_use;
            /// <summary>
            ///     Offset in sectors to start of data
            /// </summary>
            public uint data_off;
            /// <summary>
            ///     Flags
            /// </summary>
            public uint flags;
            /// <summary>
            ///     Offset in sectors to format extension
            /// </summary>
            public ulong ext_off;
        }
    }
}