// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : VDI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages VirtualBox disk images.
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
    // TODO: Support version 0
    // TODO: Support fixed images
    // TODO: Support version 1.2 geometry
    public class Vdi : IWritableImage
    {
        const uint VDI_MAGIC = 0xBEDA107F;
        const uint VDI_EMPTY = 0xFFFFFFFF;

        const string ORACLE_VDI      = "<<< Oracle VM VirtualBox Disk Image >>>\n";
        const string QEMUVDI         = "<<< QEMU VM Virtual Disk Image >>>\n";
        const string SUN_OLD_VDI     = "<<< Sun xVM VirtualBox Disk Image >>>\n";
        const string SUN_VDI         = "<<< Sun VirtualBox Disk Image >>>\n";
        const string INNOTEK_VDI     = "<<< innotek VirtualBox Disk Image >>>\n";
        const string INNOTEK_OLD_VDI = "<<< InnoTek VirtualBox Disk Image >>>\n";
        const string DIC_VDI         = "<<< DiscImageChef VirtualBox Disk Image >>>\n";

        const uint MAX_CACHE_SIZE     = 16777216;
        const uint MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;
        const uint DEFAULT_BLOCK_SIZE = 1048576;
        uint       currentWritingPosition;
        uint[]     ibm;
        ImageInfo  imageInfo;
        Stream     imageStream;

        Dictionary<ulong, byte[]> sectorCache;

        VdiHeader  vHdr;
        FileStream writingStream;

        public Vdi()
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

        public string Name => "VirtualBox Disk Image";
        public Guid   Id   => new Guid("E314DE35-C103-48A3-AD36-990F68523C46");

        public string Format => "VDI";

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

            byte[] vHdrB = new byte[Marshal.SizeOf(vHdr)];
            stream.Read(vHdrB, 0, Marshal.SizeOf(vHdr));
            vHdr             = new VdiHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vHdr));
            Marshal.Copy(vHdrB, 0, headerPtr, Marshal.SizeOf(vHdr));
            vHdr = (VdiHeader)Marshal.PtrToStructure(headerPtr, typeof(VdiHeader));
            Marshal.FreeHGlobal(headerPtr);

            return vHdr.magic == VDI_MAGIC;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] vHdrB = new byte[Marshal.SizeOf(vHdr)];
            stream.Read(vHdrB, 0, Marshal.SizeOf(vHdr));
            vHdr             = new VdiHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vHdr));
            Marshal.Copy(vHdrB, 0, headerPtr, Marshal.SizeOf(vHdr));
            vHdr = (VdiHeader)Marshal.PtrToStructure(headerPtr, typeof(VdiHeader));
            Marshal.FreeHGlobal(headerPtr);

            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.creator = {0}", vHdr.creator);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.magic = {0}",   vHdr.magic);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.version = {0}.{1}", vHdr.majorVersion,
                                      vHdr.minorVersion);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.headerSize = {0}",      vHdr.headerSize);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.imageType = {0}",       vHdr.imageType);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.imageFlags = {0}",      vHdr.imageFlags);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.description = {0}",     vHdr.comments);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.offsetBlocks = {0}",    vHdr.offsetBlocks);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.offsetData = {0}",      vHdr.offsetData);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.cylinders = {0}",       vHdr.cylinders);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.heads = {0}",           vHdr.heads);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.spt = {0}",             vHdr.spt);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.sectorSize = {0}",      vHdr.sectorSize);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.size = {0}",            vHdr.size);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blockSize = {0}",       vHdr.blockSize);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blockExtraData = {0}",  vHdr.blockExtraData);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blocks = {0}",          vHdr.blocks);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.allocatedBlocks = {0}", vHdr.allocatedBlocks);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.uuid = {0}",            vHdr.uuid);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.snapshotUuid = {0}",    vHdr.snapshotUuid);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.linkUuid = {0}",        vHdr.linkUuid);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.parentUuid = {0}",      vHdr.parentUuid);

            if(vHdr.imageType != VdiImageType.Normal)
                throw new
                    FeatureSupportedButNotImplementedImageException($"Support for image type {vHdr.imageType} not yet implemented");

            DicConsole.DebugWriteLine("VirtualBox plugin", "Reading Image Block Map");
            stream.Seek(vHdr.offsetBlocks, SeekOrigin.Begin);
            ibm         = new uint[vHdr.blocks];
            byte[] ibmB = new byte[vHdr.blocks * 4];
            stream.Read(ibmB, 0, ibmB.Length);
            for(int i = 0; i < ibm.Length; i++) ibm[i] = BitConverter.ToUInt32(ibmB, i * 4);

            sectorCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = vHdr.size / vHdr.sectorSize;
            imageInfo.ImageSize            = vHdr.size;
            imageInfo.SectorSize           = vHdr.sectorSize;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.Comments             = vHdr.comments;
            imageInfo.Version              = $"{vHdr.majorVersion}.{vHdr.minorVersion}";

            switch(vHdr.creator)
            {
                case SUN_VDI:
                    imageInfo.Application = "Sun VirtualBox";
                    break;
                case SUN_OLD_VDI:
                    imageInfo.Application = "Sun xVM";
                    break;
                case ORACLE_VDI:
                    imageInfo.Application = "Oracle VirtualBox";
                    break;
                case QEMUVDI:
                    imageInfo.Application = "QEMU";
                    break;
                case INNOTEK_VDI:
                case INNOTEK_OLD_VDI:
                    imageInfo.Application = "innotek VirtualBox";
                    break;
                case DIC_VDI:
                    imageInfo.Application = "DiscImageChef";
                    break;
            }

            imageStream = stream;

            imageInfo.Cylinders       = vHdr.cylinders;
            imageInfo.Heads           = vHdr.heads;
            imageInfo.SectorsPerTrack = vHdr.spt;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            ulong index  = sectorAddress * vHdr.sectorSize / vHdr.blockSize;
            ulong secOff = sectorAddress * vHdr.sectorSize % vHdr.blockSize;

            uint  ibmOff = ibm[index];

            if(ibmOff == VDI_EMPTY) return new byte[vHdr.sectorSize];

            ulong imageOff = vHdr.offsetData + ibmOff * vHdr.blockSize;

            byte[] cluster = new byte[vHdr.blockSize];
            imageStream.Seek((long)imageOff, SeekOrigin.Begin);
            imageStream.Read(cluster, 0, (int)vHdr.blockSize);
            sector = new byte[vHdr.sectorSize];
            Array.Copy(cluster, (int)secOff, sector, 0, vHdr.sectorSize);

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
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({sectorAddress} + {length}) than available ({imageInfo.Sectors})");

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
        public IEnumerable<string> KnownExtensions => new[] {".vdi"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize == 0)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            if(sectors * sectorSize / DEFAULT_BLOCK_SIZE > uint.MaxValue)
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

            uint ibmEntries = (uint)(sectors * sectorSize / DEFAULT_BLOCK_SIZE);
            if(sectors                       * sectorSize % DEFAULT_BLOCK_SIZE > 0) ibmEntries++;

            uint headerSectors = 1 + ibmEntries * 4 / sectorSize;
            if(ibmEntries                       * 4 % sectorSize != 0) headerSectors++;
            ibm                    = new uint[ibmEntries];
            currentWritingPosition = headerSectors * sectorSize;

            vHdr = new VdiHeader
            {
                creator      = DIC_VDI,
                magic        = VDI_MAGIC,
                majorVersion = 1,
                minorVersion = 1,
                headerSize   = Marshal.SizeOf(typeof(VdiHeader)) - 72,
                imageType    = VdiImageType.Normal,
                offsetBlocks = sectorSize,
                offsetData   = currentWritingPosition,
                sectorSize   = sectorSize,
                size         = sectors * sectorSize,
                blockSize    = DEFAULT_BLOCK_SIZE,
                blocks       = ibmEntries,
                uuid         = Guid.NewGuid(),
                snapshotUuid = Guid.NewGuid()
            };

            for(uint i = 0; i < ibmEntries; i++) ibm[i] = VDI_EMPTY;

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

            ulong index  = sectorAddress * vHdr.sectorSize / vHdr.blockSize;
            ulong secOff = sectorAddress * vHdr.sectorSize % vHdr.blockSize;

            uint ibmOff = ibm[index];

            if(ibmOff == VDI_EMPTY)
            {
                ibmOff                 =  (currentWritingPosition - vHdr.offsetData) / vHdr.blockSize;
                ibm[index]             =  ibmOff;
                currentWritingPosition += vHdr.blockSize;
                vHdr.allocatedBlocks++;
            }

            ulong imageOff = vHdr.offsetData + ibmOff * vHdr.blockSize;

            writingStream.Seek((long)imageOff, SeekOrigin.Begin);
            writingStream.Seek((long)secOff,   SeekOrigin.Current);
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

            if(!string.IsNullOrEmpty(imageInfo.Comments))
                vHdr.comments = imageInfo.Comments.Length > 255
                                    ? imageInfo.Comments.Substring(0, 255)
                                    : imageInfo.Comments;

            byte[] hdr    = new byte[Marshal.SizeOf(typeof(VdiHeader))];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VdiHeader)));
            Marshal.StructureToPtr(vHdr, hdrPtr, true);
            Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(hdr, 0, hdr.Length);

            writingStream.Seek(vHdr.offsetBlocks, SeekOrigin.Begin);
            for(long i = 0; i < ibm.LongLength; i++) writingStream.Write(BitConverter.GetBytes(ibm[i]), 0, 4);

            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            imageInfo.Comments = metadata.Comments;
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            vHdr.cylinders = cylinders;
            vHdr.heads     = heads;
            vHdr.spt       = sectorsPerTrack;
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
        ///     VDI disk image header, little-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VdiHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string creator;
            /// <summary>
            ///     Magic, <see cref="Vdi.VDI_MAGIC" />
            /// </summary>
            public uint magic;
            /// <summary>
            ///     Version
            /// </summary>
            public ushort        majorVersion;
            public ushort        minorVersion;
            public int           headerSize;
            public VdiImageType  imageType;
            public VdiImageFlags imageFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string comments;
            public uint   offsetBlocks;
            public uint   offsetData;
            public uint   cylinders;
            public uint   heads;
            public uint   spt;
            public uint   sectorSize;
            public uint   unused;
            public ulong  size;
            public uint   blockSize;
            public uint   blockExtraData;
            public uint   blocks;
            public uint   allocatedBlocks;
            public Guid   uuid;
            public Guid   snapshotUuid;
            public Guid   linkUuid;
            public Guid   parentUuid;
        }

        enum VdiImageType : uint
        {
            /// <summary> Normal dynamically growing base image file.</summary>
            Normal = 1,
            /// <summary>Preallocated base image file of a fixed size.</summary>
            Fixed,
            /// <summary>Dynamically growing image file for undo/commit changes support.</summary>
            Undo,
            /// <summary>Dynamically growing image file for differencing support.</summary>
            Differential,

            /// <summary>First valid image type value.</summary>
            First = Normal,
            /// <summary>Last valid image type value.</summary>
            Last = Differential
        }

        enum VdiImageFlags : uint
        {
            /// <summary>
            ///     Fill new blocks with zeroes while expanding image file. Only valid for newly created images, never set
            ///     for opened existing images.
            /// </summary>
            ZeroExpand = 0x100
        }
    }
}